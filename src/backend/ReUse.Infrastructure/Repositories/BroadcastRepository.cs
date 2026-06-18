using Microsoft.EntityFrameworkCore;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Broadcast;
using ReUse.Application.Interfaces.Repository;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;
using ReUse.Infrastructure.Extensions;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Repositories;

public class BroadcastRepository : BaseRepository<BroadcastMessage>, IBroadcastRepository
{
    private readonly ApplicationDbContext _db;

    public BroadcastRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<PagedResult<BroadcastMessage>> GetAllAsync(BroadcastFilterParams filterParams)
    {
        var query = _db.BroadcastMessages
            .AsNoTracking()
            .Include(b => b.CreatedBy)
            .AsQueryable();

        if (filterParams.Status.HasValue)
            query = query.Where(b => b.Status == filterParams.Status.Value);

        query = query.OrderByDescending(b => b.CreatedAt);

        return await query.ToPagedListAsync(filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<BroadcastMessage?> GetByIdWithCreatorAsync(Guid id)
    {
        return await _db.BroadcastMessages
            .Include(b => b.CreatedBy)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<BroadcastSummaryStats> GetSummaryStatsAsync()
    {
        var totalSent = await _db.BroadcastMessages.CountAsync(b => b.Status == BroadcastStatus.Sent);
        var totalScheduled = await _db.BroadcastMessages.CountAsync(b => b.Status == BroadcastStatus.Scheduled);
        var totals = await _db.BroadcastMessages
            .Where(b => b.Status == BroadcastStatus.Sent)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Recipients = (int)g.Sum(b => b.RecipientCount),
                Delivered = (int)g.Sum(b => b.DeliveredCount)
            })
            .FirstOrDefaultAsync();

        return new BroadcastSummaryStats
        {
            TotalSent = totalSent,
            TotalScheduled = totalScheduled,
            TotalRecipients = totals?.Recipients ?? 0,
            TotalDelivered = totals?.Delivered ?? 0
        };
    }

    public async Task<List<BroadcastMessage>> GetDueScheduledAsync(DateTime utcNow)
    {
        return await _db.BroadcastMessages
            .Where(b => b.Status == BroadcastStatus.Scheduled && b.ScheduledAt <= utcNow)
            .ToListAsync();
    }
}