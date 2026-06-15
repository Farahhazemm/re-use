using Microsoft.EntityFrameworkCore;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.SystemActivityLog;
using ReUse.Application.Enums;
using ReUse.Application.Interfaces.Repository;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;
using ReUse.Infrastructure.Extensions;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Repositories;

public class SystemActivityLogRepository : BaseRepository<SystemActivityLog>, ISystemActivityLogRepository
{
    private readonly ApplicationDbContext _context;

    public SystemActivityLogRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<SystemActivityLogResponse>> GetAllAsync(SystemActivityLogFilterParams filterParams)
    {
        var query = _context.SystemActivityLogs
            .AsNoTracking()
            .AsQueryable();

        if (filterParams.ActorUserId.HasValue)
            query = query.Where(l => l.ActorUserId == filterParams.ActorUserId.Value);

        if (filterParams.ActionType.HasValue)
            query = query.Where(l => l.ActionType == filterParams.ActionType.Value);

        if (filterParams.Category.HasValue)
            query = query.Where(l => l.Category == filterParams.Category.Value);

        if (filterParams.Severity.HasValue)
            query = query.Where(l => l.Severity == filterParams.Severity.Value);

        if (filterParams.Status.HasValue)
            query = query.Where(l => l.Status == filterParams.Status.Value);

        if (!string.IsNullOrWhiteSpace(filterParams.EntityType))
            query = query.Where(l => l.EntityType == filterParams.EntityType);

        if (!string.IsNullOrWhiteSpace(filterParams.EntityId))
            query = query.Where(l => l.EntityId == filterParams.EntityId);

        if (filterParams.CreatedFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= filterParams.CreatedFrom.Value);

        if (filterParams.CreatedTo.HasValue)
            query = query.Where(l => l.CreatedAt <= filterParams.CreatedTo.Value);

        var keyword = filterParams.Search ?? filterParams.DescriptionSearch;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(l =>
                EF.Functions.ILike(l.Description, $"%{keyword}%") ||
                EF.Functions.ILike(l.ActionType.ToString(), $"%{keyword}%") ||
                (l.EntityType != null && EF.Functions.ILike(l.EntityType, $"%{keyword}%")));
        }

        query = (filterParams.SortBy?.ToLowerInvariant()) switch
        {
            "oldest" => query.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id),

            "severity" => query
                .OrderByDescending(l =>
                    l.Severity == LogSeverity.Critical ? 3 :
                    l.Severity == LogSeverity.Error ? 2 :
                    l.Severity == LogSeverity.Warning ? 1 : 0)
                .ThenByDescending(l => l.CreatedAt)
                .ThenByDescending(l => l.Id),

            _ => query.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id)
        };

        return await query
            .Select(l => new SystemActivityLogResponse
            {
                Id = l.Id,
                ActorUserId = l.ActorUserId,

                ActorName = l.ActorName,
                ActorEmail = l.ActorEmail,

                ActionType = l.ActionType,
                Category = l.Category,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Severity = l.Severity,
                Status = l.Status,
                Description = l.Description,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                Metadata = l.Metadata,
                CreatedAt = l.CreatedAt
            })
            .ToPagedListAsync(
                filterParams.Pagination.PageNumber,
                filterParams.Pagination.PageSize);
    }

    public async Task<SystemActivityLog?> GetByIdDetailAsync(Guid id)
    {
        return await _context.SystemActivityLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }
}