using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

using Reuse.Infrastructure.Identity.Models;

using ReUse.Application.DTOs.Users.AccountManagement;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;
using ReUse.Application.Interfaces.Services.External;
using ReUse.Domain.Entities;
using ReUse.Infrastructure.Interfaces.Services;

namespace ReUse.Infrastructure.Services.Identity;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IDistributedCache _cache;
    private readonly ISystemActivityLogService _activityLog;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IDistributedCache cache,
        ISystemActivityLogService activityLog)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _cache = cache;
        _activityLog = activityLog;
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException(nameof(ApplicationUser));

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new IdentityOperationException(errors);
        }

        var domainUser = await _unitOfWork.User.GetByIdentityIdAsync(userId);
        if (domainUser is not null)
            await _activityLog.LogPasswordChangedAsync(domainUser.Id);
    }

    public async Task DeactivateAccountAsync(Guid userId, DeactivateAccountRequest request)
    {
        var user = await _unitOfWork.User.GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User));

        if (!user.IsActive)
            throw new ConflictException("Account is already deactivated.");

        var identityUser = await _userManager.FindByIdAsync(user.IdentityUserId)
            ?? throw new NotFoundException(nameof(ApplicationUser));

        var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
        if (!passwordValid)
            throw new ForbiddenException();

        user.IsActive = false;
        user.DeactivatedAt = DateTime.UtcNow;
        user.DeactivationReason = request.Reason;

        _tokenService.RevokeAllAsync(identityUser);
        await _userManager.UpdateSecurityStampAsync(identityUser);

        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"user:active:{userId}");
    }

    public async Task EnsureActiveOnLoginAsync(Guid userId)
    {
        var user = await _unitOfWork.User.GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User));

        var identityUser = await _userManager.FindByIdAsync(user.IdentityUserId)
            ?? throw new NotFoundException(nameof(ApplicationUser));

        if (await _userManager.IsLockedOutAsync(identityUser))
            throw new UserBlockedException();

        if (user.IsActive) return;

        user.IsActive = true;
        user.DeactivatedAt = null;
        user.DeactivationReason = null;

        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"user:active:{userId}");
    }

    public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request)
    {
        var user = await _unitOfWork.User.GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User));

        var identityUser = await _userManager.FindByIdAsync(user.IdentityUserId)
            ?? throw new NotFoundException(nameof(ApplicationUser));

        var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
        if (!passwordValid)
            throw new ForbiddenException();

        var actorId = user.Id;
        var actorEmail = identityUser.Email ?? string.Empty;
        var actorName = $"{user.FullName}".Trim();

        await _activityLog.LogAccountDeletedAsync(actorId, actorEmail, actorName);

        await _unitOfWork.Conversation.DeleteByUserIdAsync(userId);
        await _unitOfWork.Product.DeleteByUserIdAsync(userId);
        await _unitOfWork.Follow.DeleteByUserIdAsync(userId);
        await _unitOfWork.Comments.DeleteByUserIdAsync(userId);

        _unitOfWork.User.Remove(user);
        await _unitOfWork.SaveChangesAsync();

        var result = await _userManager.DeleteAsync(identityUser);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new IdentityOperationException(errors);
        }

        await _cache.RemoveAsync($"user:active:{userId}");

    }
}