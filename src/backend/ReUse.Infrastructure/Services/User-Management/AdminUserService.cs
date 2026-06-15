using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

using Reuse.Infrastructure.Identity.Models;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Users.Admin;
using ReUse.Application.DTOs.Users.User_Management;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;
using ReUse.Application.Mappers;
using ReUse.Domain.Entities;
using ReUse.Infrastructure.Identity;
using ReUse.Infrastructure.Interfaces.Repositories;
using ReUse.Infrastructure.Interfaces.Services;

namespace ReUse.Infrastructure.Services.User_Management;

public class AdminUserService : IAdminUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityUserRepository _identityUserRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IDistributedCache _cache;
    private readonly IMapper _mapper;
    private readonly ISystemActivityLogService _activityLog;

    public AdminUserService(
        IUnitOfWork unitOfWork,
        IIdentityUserRepository identityUserRepo,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IDistributedCache cache,
        IMapper mapper,
        ISystemActivityLogService activityLog)
    {
        _unitOfWork = unitOfWork;
        _identityUserRepo = identityUserRepo;
        _userManager = userManager;
        _tokenService = tokenService;
        _cache = cache;
        _mapper = mapper;
        _activityLog = activityLog;
    }

    #region GetAllUsers
    public async Task<PagedResult<AdminUserResponse>> GetAllUsersAsync(
        AdminUserFilterParams filterParams,
        Guid currentAdminId)
    {
        var currentAdmin = await _unitOfWork.User.GetByIdAsync(currentAdminId)
            ?? throw new NotFoundException("Current admin domain user not found");

        HashSet<string>? allowedIdentityIds = null;

        var roleName = UserRoleMapper.ToRoleName(filterParams.Role);
        if (roleName is not null)
        {
            allowedIdentityIds = await _identityUserRepo.GetUserIdsByRoleAsync(roleName);

            if (allowedIdentityIds.Count == 0)
                return new PagedResult<AdminUserResponse>
                {
                    Data = [],
                    PageNumber = filterParams.Pagination.PageNumber,
                    PageSize = filterParams.Pagination.PageSize,
                    TotalRecords = 0
                };
        }

        var pagedDomainUsers = await _unitOfWork.User
            .GetPagedAdminAsync(filterParams, allowedIdentityIds, excludeUserId: currentAdmin.Id);

        if (pagedDomainUsers.Data.Count == 0)
            return new PagedResult<AdminUserResponse>
            {
                Data = [],
                PageNumber = pagedDomainUsers.PageNumber,
                PageSize = pagedDomainUsers.PageSize,
                TotalRecords = pagedDomainUsers.TotalRecords
            };

        var identityIds = pagedDomainUsers.Data.Select(u => u.IdentityUserId).ToList();
        var rolesMap = await _identityUserRepo.GetRolesByUserIdsAsync(identityIds);

        var responseDtos = pagedDomainUsers.Data
            .Select(domainUser =>
            {
                rolesMap.TryGetValue(domainUser.IdentityUserId, out var roles);
                return _mapper.Map<AdminUserResponse>(new AdminUserMappingSource
                {
                    DomainUser = domainUser,
                    Roles = roles ?? []
                });
            })
            .ToList();

        return new PagedResult<AdminUserResponse>
        {
            Data = responseDtos,
            PageNumber = pagedDomainUsers.PageNumber,
            PageSize = pagedDomainUsers.PageSize,
            TotalRecords = pagedDomainUsers.TotalRecords
        };
    }
    #endregion

    #region CreateNewUser
    public async Task<AdminUserResponse> CreateUserAsync(CreateAdminUserRequest request)
    {
        var existingIdentity = await _identityUserRepo.GetByEmail(request.Email);
        if (existingIdentity is not null)
            throw new ConflictException(nameof(ApplicationUser));

        var identityUser = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var createResult = await _identityUserRepo.CreateAsync(identityUser, request.Password);
        if (!createResult.Succeeded)
            throw new IdentityOperationException(createResult.Errors.Select(e => e.Description));

        var roleName = UserRoleMapper.ToRoleName(request.Role);
        var roleResult = await _identityUserRepo.AddToRoleAsync(identityUser, roleName);
        if (!roleResult.Succeeded)
            throw new IdentityOperationException(roleResult.Errors.Select(e => e.Description));

        var businessUser = new User
        {
            IdentityUserId = identityUser.Id,
            FullName = request.FullName,
            Email = request.Email,
            IsActive = true
        };

        _unitOfWork.User.Add(businessUser);
        await _unitOfWork.SaveChangesAsync();

        var claimResult = await _identityUserRepo.AddClaimAsync(
            identityUser,
            new Claim("business_user_id", businessUser.Id.ToString()));

        if (!claimResult.Succeeded)
            throw new IdentityOperationException(claimResult.Errors.Select(e => e.Description));

        return _mapper.Map<AdminUserResponse>(new AdminUserMappingSource
        {
            DomainUser = businessUser,
            Roles = [roleName]
        });
    }
    #endregion

    #region UpdateUser
    public async Task<AdminUserResponse> UpdateUserAsync(Guid userId, UpdateAdminUserRequest request, Guid currentAdminId)
    {
        var currentAdminDomainUser = await _unitOfWork.User.GetByIdAsync(currentAdminId);

        if (currentAdminDomainUser is not null && userId == currentAdminDomainUser.Id)
            throw new BadRequestException("You cannot Update your own account.");

        var domainUser = await _unitOfWork.User.GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User));

        if (request.FullName is not null) domainUser.FullName = request.FullName;
        if (request.PhoneNumber is not null) domainUser.PhoneNumber = request.PhoneNumber;
        if (request.Bio is not null) domainUser.Bio = request.Bio;
        if (request.AddressLine1 is not null) domainUser.AddressLine1 = request.AddressLine1;
        if (request.City is not null) domainUser.City = request.City;
        if (request.StateProvince is not null) domainUser.StateProvince = request.StateProvince;
        if (request.PostalCode is not null) domainUser.PostalCode = request.PostalCode;
        if (request.Country is not null) domainUser.Country = request.Country;

        List<string> currentRoles;

        if (request.Role.HasValue)
        {
            var identityUser = await _identityUserRepo.GetByIdAsync(domainUser.IdentityUserId)
                ?? throw new NotFoundException(nameof(ApplicationUser));

            var existingRoles = await _identityUserRepo.GetRolesAsync(identityUser);
            var newRoleName = UserRoleMapper.ToRoleName(request.Role.Value);

            if (!existingRoles.Contains(newRoleName))
            {
                foreach (var role in existingRoles)
                    await _identityUserRepo.RemoveFromRoleAsync(identityUser, role);

                var addResult = await _identityUserRepo.AddToRoleAsync(identityUser, newRoleName);
                if (!addResult.Succeeded)
                    throw new IdentityOperationException(addResult.Errors.Select(e => e.Description));
            }

            currentRoles = [newRoleName];
        }
        else
        {
            var identityIds = new List<string> { domainUser.IdentityUserId };
            var rolesMap = await _identityUserRepo.GetRolesByUserIdsAsync(identityIds);
            rolesMap.TryGetValue(domainUser.IdentityUserId, out var roles);
            currentRoles = roles ?? [];
        }

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AdminUserResponse>(new AdminUserMappingSource
        {
            DomainUser = domainUser,
            Roles = currentRoles
        });
    }
    #endregion

    #region DeleteUser
    public async Task DeleteUserAsync(Guid userId, Guid currentAdminId)
    {
        var domainUser = await _unitOfWork.User.GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User));

        var currentAdminDomainUser = await _unitOfWork.User.GetByIdAsync(currentAdminId);

        if (currentAdminDomainUser is not null && userId == currentAdminDomainUser.Id)
            throw new BadRequestException("You cannot delete your own account.");

        var identityUser = await _userManager.FindByIdAsync(domainUser.IdentityUserId)
            ?? throw new NotFoundException(nameof(ApplicationUser));

        await _unitOfWork.Follow.DeleteByUserIdAsync(userId);
        _unitOfWork.User.Remove(domainUser);

        _tokenService.RevokeAllAsync(identityUser);
        var result = await _userManager.DeleteAsync(identityUser);
        if (!result.Succeeded)
            throw new IdentityOperationException(result.Errors.Select(e => e.Description));

        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"user:active:{userId}");
    }
    #endregion

    #region BlockUser
    public async Task<AdminUserResponse> BlockUserAsync(Guid userId, Guid currentAdminId)
    {
        var domainUser = await _unitOfWork.User.GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User));

        if (userId == currentAdminId)
            throw new BadRequestException("You cannot block your own account.");

        if (!domainUser.IsActive)
            throw new ConflictException("User is already blocked.");

        var identityUser = await _identityUserRepo.GetByIdWithRefreshTokens(domainUser.IdentityUserId)
            ?? throw new NotFoundException(nameof(ApplicationUser));

        domainUser.IsActive = false;
        domainUser.DeactivatedAt = DateTime.UtcNow;

        await _userManager.SetLockoutEnabledAsync(identityUser, true);
        await _userManager.SetLockoutEndDateAsync(identityUser, DateTimeOffset.MaxValue);

        _tokenService.RevokeAllAsync(identityUser);
        await _identityUserRepo.UpdateAsync(identityUser);

        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"user:active:{userId}");

        await _activityLog.LogUserBlockedAsync(currentAdminId, userId);

        var identityIds = new List<string> { domainUser.IdentityUserId };
        var rolesMap = await _identityUserRepo.GetRolesByUserIdsAsync(identityIds);
        rolesMap.TryGetValue(domainUser.IdentityUserId, out var roles);

        return _mapper.Map<AdminUserResponse>(new AdminUserMappingSource
        {
            DomainUser = domainUser,
            Roles = roles ?? []
        });
    }
    #endregion

    #region UnlockUser
    public async Task<AdminUserResponse> UnlockUserAsync(Guid userId, Guid currentAdminId)
    {
        var domainUser = await _unitOfWork.User.GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User));

        if (userId == currentAdminId)
            throw new BadRequestException("You cannot unlock your own account.");

        var identityUser = await _identityUserRepo.GetByIdWithRefreshTokens(domainUser.IdentityUserId)
            ?? throw new NotFoundException(nameof(ApplicationUser));

        domainUser.IsActive = true;
        domainUser.DeactivatedAt = null;
        domainUser.DeactivationReason = null;

        await _userManager.SetLockoutEndDateAsync(identityUser, null);
        await _userManager.ResetAccessFailedCountAsync(identityUser);

        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"user:active:{userId}");

        await _activityLog.LogUserUnblockedAsync(currentAdminId, userId);

        var identityIds = new List<string> { domainUser.IdentityUserId };
        var rolesMap = await _identityUserRepo.GetRolesByUserIdsAsync(identityIds);
        rolesMap.TryGetValue(domainUser.IdentityUserId, out var roles);

        return _mapper.Map<AdminUserResponse>(new AdminUserMappingSource
        {
            DomainUser = domainUser,
            Roles = roles ?? []
        });
    }
    #endregion
}