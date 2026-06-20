using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using ReUse.Application.DTOs;
using ReUse.Application.Enums;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces.Repository;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;
using ReUse.Infrastructure.Extensions;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Repositories;

public class ReportRepository : BaseRepository<Report>, IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }


    public async Task AddReportAsync(Report report)
    {
        try
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Report");
        }
    }
    public async Task<bool> ExistsByReporterAsync(Guid reporterUserId, ReportTargetType targetType, Guid targetId)
    {
        return await _context.Reports
            .AsNoTracking()
            .AnyAsync(r => r.ReporterUserId == reporterUserId
                        && r.TargetType == targetType
                        && r.TargetId == targetId);
    }

    public async Task<Report?> GetByIdWithDetailsAsync(Guid reportId)
    {
        return await _context.Reports
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedBy)
            .FirstOrDefaultAsync(r => r.Id == reportId);
    }

    public async Task<PagedResult<Report>> GetAllAsync(
    ReportStatus? status,
    ReportTargetType? targetType,
    Guid? reporterUserId,
    DateTime? createdFrom,
    DateTime? createdTo,
    PaginationParams pagination,
    SortDirection sortDirection = SortDirection.Desc)
    {
        var query = _context.Reports
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedBy)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (targetType.HasValue)
            query = query.Where(r => r.TargetType == targetType.Value);

        query = sortDirection == SortDirection.Asc
            ? query.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
            : query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);
        if (reporterUserId.HasValue)
            query = query.Where(r => r.ReporterUserId == reporterUserId);

        if (createdFrom.HasValue)
            query = query.Where(r => r.CreatedAt >= createdFrom.Value);

        if (createdTo.HasValue)
            query = query.Where(r => r.CreatedAt <= createdTo.Value);

        return await query.ToPagedListAsync(pagination.PageNumber, pagination.PageSize);
    }

    public async Task<PagedResult<Report>> GetByTargetAsync(
        ReportTargetType targetType,
        Guid targetId,
        PaginationParams pagination,
        SortDirection sortDirection = SortDirection.Desc)
    {
        var query = _context.Reports
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Where(r => r.TargetType == targetType && r.TargetId == targetId);

        query = sortDirection == SortDirection.Asc
            ? query.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
            : query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);

        return await query.ToPagedListAsync(pagination.PageNumber, pagination.PageSize);
    }

    public async Task<PagedResult<Report>> GetByReporterAsync(
        Guid reporterUserId,
        PaginationParams pagination,
        SortDirection sortDirection = SortDirection.Desc)
    {
        var query = _context.Reports
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Where(r => r.ReporterUserId == reporterUserId);

        query = sortDirection == SortDirection.Asc
            ? query.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
            : query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);

        return await query.ToPagedListAsync(pagination.PageNumber, pagination.PageSize);
    }

    public async Task<int> CountPendingByTargetAsync(ReportTargetType targetType, Guid targetId)
    {
        return await _context.Reports
            .AsNoTracking()
            .CountAsync(r => r.TargetType == targetType
                          && r.TargetId == targetId
                          && r.Status == ReportStatus.Pending);
    }

    public async Task<int> CountByStatusAsync(ReportStatus status, DateTime? from, DateTime? to)
    {
        var query = _context.Reports
            .AsNoTracking()
            .Where(r => r.Status == status);

        if (from.HasValue)
            query = query.Where(r => r.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.CreatedAt <= to.Value);

        return await query.CountAsync();
    }

    public async Task<int> CountCurrentlyByStatusAsync(ReportStatus status)
    {
        return await _context.Reports
            .AsNoTracking()
            .CountAsync(r => r.Status == status);
    }
}