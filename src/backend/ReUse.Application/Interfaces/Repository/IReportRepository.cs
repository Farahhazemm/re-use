using ReUse.Application.DTOs;
using ReUse.Application.Enums;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Application.Interfaces.Repository;

public interface IReportRepository : IBaseRepository<Report>
{
    public Task AddReportAsync(Report report);
    Task<bool> ExistsByReporterAsync(Guid reporterUserId, ReportTargetType targetType, Guid targetId);

    Task<Report?> GetByIdWithDetailsAsync(Guid reportId);

    public Task<PagedResult<Report>> GetAllAsync(ReportStatus? status, ReportTargetType? targetType, Guid? reporterUserId, DateTime? createdFrom, DateTime? createdTo, PaginationParams pagination, SortDirection sortDirection = SortDirection.Desc);
    Task<PagedResult<Report>> GetByTargetAsync(ReportTargetType targetType, Guid targetId, PaginationParams pagination, SortDirection sortDirection = SortDirection.Desc);

    Task<PagedResult<Report>> GetByReporterAsync(Guid reporterUserId, PaginationParams pagination, SortDirection sortDirection = SortDirection.Desc);

    Task<int> CountPendingByTargetAsync(ReportTargetType targetType, Guid targetId);
}