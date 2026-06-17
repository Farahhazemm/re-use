using Microsoft.EntityFrameworkCore;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Users.Admin;
using ReUse.Application.Interfaces.Repository;
using ReUse.Domain.Entities;
using ReUse.Infrastructure.Extensions;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdentityIdAsync(string identityUserId)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
    }

    public async Task<string?> GetIdentityUserIdAsync(Guid userId)
    {
        return await _context.Set<User>()
            .Where(u => u.Id == userId)
            .Select(u => u.IdentityUserId)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetProfileByIdAsync(Guid userId)
    {
        return await _context.Set<User>()
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public Task<PagedResult<User>> GetPagedAdminAsync(
        AdminUserFilterParams filterParams,
        HashSet<string>? allowedIdentityIds,
        Guid? excludeUserId)
    {
        var query = _context.Set<User>().AsNoTracking().AsQueryable();

        if (allowedIdentityIds is not null)
            query = query.Where(u => allowedIdentityIds.Contains(u.IdentityUserId));

        if (excludeUserId.HasValue)
            query = query.ExcludeUser(excludeUserId.Value);

        query = query
            .Search(filterParams.SearchTerm)
            .FilterByCity(filterParams.City)
            .FilterByCountry(filterParams.Country)
            .FilterByStateProvince(filterParams.StateProvince)
            .FilterByActive(filterParams.IsActive)
            .FilterByCreatedDate(filterParams.CreatedAfter, filterParams.CreatedBefore)
            .ApplySort(filterParams.SortBy, filterParams.SortOrder);

        return query.ToPagedListAsync(
            filterParams.Pagination.PageNumber,
            filterParams.Pagination.PageSize);
    }

    public async Task<List<Guid>> GetIdsByIdentityIdsAsync(IEnumerable<string> identityIds)
    {
        var idSet = identityIds.ToHashSet();
        return await _context.Set<User>()
            .AsNoTracking()
            .Where(u => u.IsActive && idSet.Contains(u.IdentityUserId))
            .Select(u => u.Id)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetAllActiveUserIdsAsync()
    {
        return await _context.Set<User>()
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();
    }
}