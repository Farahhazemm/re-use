using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Users.Admin;
using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByIdentityIdAsync(string identityUserId);
    Task<string?> GetIdentityUserIdAsync(Guid userId);
    Task<User?> GetProfileByIdAsync(Guid userId);
    Task<PagedResult<User>> GetPagedAdminAsync(AdminUserFilterParams filterParams, HashSet<string>? allowedIdentityIds, Guid? excludeUserId = null);
}