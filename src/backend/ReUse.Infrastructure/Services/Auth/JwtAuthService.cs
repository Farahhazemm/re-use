using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;

using Reuse.Infrastructure.Identity.Models;

using ReUse.Application.DTOs.Auth;
using ReUse.Application.DTOs.Users.UserProfile;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;
using ReUse.Application.Interfaces.Services.External;
using ReUse.Domain.Entities;
using ReUse.Infrastructure.Interfaces.Repositories;
using ReUse.Infrastructure.Interfaces.Services;

namespace ReUse.Infrastructure.Services.Auth;

// TODO: Inject IRequestContext once available so IpAddress/UserAgent are resolved centrally
//       rather than being absent from auth-level log calls.

public class JwtAuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IIdentityUserRepository _identityUserRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAccountService _accountService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMapper _mapper;
    private readonly ISystemActivityLogService _activityLog;

    public JwtAuthService(
        IUnitOfWork uow,
        ITokenService tokenService,
        IIdentityUserRepository identityUserRepo,
        IMapper mapper,
        IAccountService accountService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISystemActivityLogService activityLog)
    {
        _uow = uow;
        _tokenService = tokenService;
        _identityUserRepo = identityUserRepo;
        _accountService = accountService;
        _mapper = mapper;
        _userManager = userManager;
        _signInManager = signInManager;
        _activityLog = activityLog;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _identityUserRepo.GetByEmail(request.Email);

        if (user == null)
        {
            await _activityLog.LogLoginFailedAsync(request.Email, reason: "User not found.");
            throw new InvalidCredentialsException();
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

            if (lockoutEnd == DateTimeOffset.MaxValue)
            {
                await _activityLog.LogLoginFailedAsync(request.Email, reason: "Account blocked by admin.");
                throw new UserBlockedException();
            }

            await _activityLog.LogLoginFailedAsync(request.Email, reason: "Account temporarily locked out.");
            throw new UserLockedOutException();
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            await _activityLog.LogLoginFailedAsync(request.Email, reason: "Locked out after failed attempts.");
            throw new UserLockedOutException();
        }

        if (!signInResult.Succeeded)
        {
            await _activityLog.LogLoginFailedAsync(request.Email, reason: "Invalid credentials.");
            throw new InvalidCredentialsException();
        }

        if (!user.EmailConfirmed)
        {
            await _activityLog.LogLoginFailedAsync(request.Email, reason: "Email not confirmed.");
            throw new EmailNotConfirmedException();
        }

        var domainUser = await _uow.User.GetByIdentityIdAsync(user.Id)
            ?? throw new NotFoundException(nameof(User));

        await _accountService.EnsureActiveOnLoginAsync(domainUser.Id);

        var jwtToken = await _tokenService.GenerateJwtAsync(user);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user);

        await _activityLog.LogLoginSuccessAsync(domainUser.Id);

        return new LoginResponse(
            user.Email!,
            new JwtSecurityTokenHandler().WriteToken(jwtToken),
            refreshToken.Token,
            jwtToken.ValidTo,
            refreshToken.ExpiresAt);
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest refreshToken)
    {
        var user = await _identityUserRepo.GetByRefreshTokenWithRefreshTokens(refreshToken.RefreshToken);

        if (user == null)
            throw new InvalidRefreshTokenException();

        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user, refreshToken.RefreshToken);
        var jwtToken = await _tokenService.GenerateJwtAsync(user);

        return new LoginResponse(
            user.Email!,
            new JwtSecurityTokenHandler().WriteToken(jwtToken),
            newRefreshToken.Token,
            jwtToken.ValidTo,
            newRefreshToken.ExpiresAt);
    }

    public async Task LogoutAsync(string identityUserId)
    {
        var user = await _identityUserRepo.GetByIdWithRefreshTokens(identityUserId);
        if (user == null) return;

        _tokenService.RevokeAllAsync(user);
        await _identityUserRepo.UpdateAsync(user);
    }

    public async Task<UserProfileResponse> RegisterAsync(RegisterRequest request)
    {
        ApplicationUser? identityUser = null;
        User? businessUser = null;

        try
        {
            identityUser = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var createUserResult = await _identityUserRepo.CreateAsync(identityUser, request.Password);

            if (!createUserResult.Succeeded)
                throw new IdentityOperationException(createUserResult.Errors.Select(e => e.Description));

            var roleResult = await _identityUserRepo.AddToRoleAsync(identityUser, "User");

            if (!roleResult.Succeeded)
                throw new IdentityOperationException(roleResult.Errors.Select(e => e.Description));

            businessUser = new User
            {
                IdentityUserId = identityUser.Id,
                Email = identityUser.Email,
                FullName = request.FullName
            };

            _uow.User.Add(businessUser);
            await _uow.SaveChangesAsync();

            var claimResult = await _identityUserRepo.AddClaimAsync(
                identityUser,
                new Claim("business_user_id", businessUser.Id.ToString()));

            if (!claimResult.Succeeded)
                throw new IdentityOperationException(claimResult.Errors.Select(e => e.Description));

            return _mapper.Map<UserProfileResponse>(businessUser);
        }
        catch
        {
            // Compensation logic
            if (businessUser != null)
            {
                _uow.User.Remove(businessUser);
                await _uow.SaveChangesAsync();
            }

            if (identityUser != null)
                await _identityUserRepo.DeleteAsync(identityUser);

            throw;
        }
    }
}