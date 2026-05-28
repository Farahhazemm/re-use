using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Reuse.Infrastructure.Identity.Models;

using ReUse.Infrastructure.Identity;
using ReUse.Infrastructure.Interfaces.Repositories;

namespace ReUse.Infrastructure.Repositories;

public class IdentityUserRepository : IIdentityUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppIdentityDbContext _identityDb;

    public IdentityUserRepository(
        UserManager<ApplicationUser> userManager,
        AppIdentityDbContext identityDb)
    {
        _userManager = userManager;
        _identityDb = identityDb;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        => await _userManager.CreateAsync(user, password);

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
        => await _userManager.UpdateAsync(user);

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
        => await _userManager.DeleteAsync(user);

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        => await _userManager.AddToRoleAsync(user, role);

    public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
        => await _userManager.RemoveFromRoleAsync(user, role);

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        => await _userManager.GetRolesAsync(user);

    public async Task<IdentityResult> AddClaimAsync(ApplicationUser user, Claim claim)
        => await _userManager.AddClaimAsync(user, claim);

    public async Task<ApplicationUser?> GetByEmail(string email)
        => await _userManager.FindByEmailAsync(email);

    public async Task<ApplicationUser?> GetByEmailWithRefreshTokens(string email)
        => await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == email);

    public async Task<ApplicationUser?> GetByRefreshTokenWithRefreshTokens(string refreshToken)
        => await _userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

    public async Task<ApplicationUser?> GetByIdAsync(string id)
        => await _userManager.FindByIdAsync(id);

    public async Task<ApplicationUser?> GetByIdWithRefreshTokens(string id)
        => await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == id);

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        => await _userManager.CheckPasswordAsync(user, password);

    // Admin bulk helpers

    public async Task<List<ApplicationUser>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var idSet = ids as ICollection<string> ?? ids.ToList();
        return await _userManager.Users
            .AsNoTracking()
            .Where(u => idSet.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<string>>> GetRolesByUserIdsAsync(IEnumerable<string> identityUserIds)
    {
        var idSet = identityUserIds as ICollection<string> ?? identityUserIds.ToList();

        var pairs = await (
            from ur in _identityDb.UserRoles.AsNoTracking()
            join r in _identityDb.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where idSet.Contains(ur.UserId)
            select new { ur.UserId, RoleName = r.Name! }
        ).ToListAsync();

        return pairs
            .GroupBy(p => p.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.RoleName).ToList());
    }

    public async Task<HashSet<string>> GetUserIdsByRoleAsync(string roleName)
    {
        // Single Join to resolve role name => role ID => user Ids
        var ids = await (
            from r in _identityDb.Roles.AsNoTracking()
            join ur in _identityDb.UserRoles.AsNoTracking() on r.Id equals ur.RoleId
            where r.NormalizedName == roleName.ToUpperInvariant()
            select ur.UserId
        ).ToListAsync();

        return [.. ids];
    }
}