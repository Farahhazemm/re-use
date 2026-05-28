using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Reuse.Infrastructure.Identity.Models;

namespace ReUse.Infrastructure.Interfaces.Repositories;

public interface IIdentityUserRepository
{
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<IdentityResult> AddClaimAsync(ApplicationUser user, Claim claim);
    Task<ApplicationUser?> GetByEmail(string email);
    Task<ApplicationUser?> GetByEmailWithRefreshTokens(string email);
    Task<ApplicationUser?> GetByRefreshTokenWithRefreshTokens(string refreshToken);
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByIdWithRefreshTokens(string id);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

    // Admin bulk role lookup
    Task<List<ApplicationUser>> GetByIdsAsync(IEnumerable<string> ids);
    Task<Dictionary<string, List<string>>> GetRolesByUserIdsAsync(IEnumerable<string> identityUserIds);
    Task<HashSet<string>> GetUserIdsByRoleAsync(string roleName);
}