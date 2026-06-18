using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using ReUse.Application.Interfaces.Repository;

using ReUse.Domain.Entities;

using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Repositories;

public class ActivityRepository : BaseRepository<ActivityEvent>, IActivityRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<ActivityEvent>> GetByUserIdAsync(Guid userId, int limit = 50)
    {
        return await _context.Set<ActivityEvent>()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<(List<ActivityEvent> Items, bool HasMore)> GetHistoryAsync(
        Guid userId, int limit, DateTime? before, DateTime? from, DateTime? to, string? type)
    {
        IQueryable<ActivityEvent> query = _context.Set<ActivityEvent>()
            .Include(a => a.Product)
                .ThenInclude(p => p!.ProductImages)
            .Include(a => a.Product)
                .ThenInclude(p => p!.Owner)
            .Where(a => a.UserId == userId);

        if (before.HasValue)
            query = query.Where(a => a.Timestamp < before.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(a => a.Type.StartsWith(type + ".") || a.Type == type);

        // Dedup at database level: for product.viewed, only keep latest occurrence per product
        query = query.Where(a => a.Type != "product.viewed" || !_context.Set<ActivityEvent>()
            .Any(a2 => a2.UserId == userId && a2.Type == "product.viewed"
                && a2.ProductId == a.ProductId && a2.Timestamp > a.Timestamp));

        query = query.OrderByDescending(a => a.Timestamp);

        // Fetch limit + 1 to determine HasMore
        var items = await query.Take(limit + 1).ToListAsync();

        var hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return (items, hasMore);
    }
}