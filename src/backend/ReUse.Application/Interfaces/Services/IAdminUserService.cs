using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Users.Admin;
using ReUse.Application.DTOs.Users.User_Management;

namespace ReUse.Application.Interfaces.Services;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserResponse>> GetAllUsersAsync(AdminUserFilterParams filterParams, Guid currentAdminId);
    Task<AdminUserResponse> CreateUserAsync(CreateAdminUserRequest request);
    Task<AdminUserResponse> UpdateUserAsync(Guid userId, UpdateAdminUserRequest request, Guid currentAdminId);
    Task DeleteUserAsync(Guid userId, Guid currentAdminId);

    Task<AdminUserResponse> BlockUserAsync(Guid userId, Guid currentAdminId);
    Task<AdminUserResponse> UnlockUserAsync(Guid userId, Guid currentAdminId);
}