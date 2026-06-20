using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Reports;
using ReUse.Application.Enums;
using ReUse.Domain.Enums;

namespace ReUse.Application.Interfaces.Services;

public interface IReportService
{
    Task<ReportDetailsResponse> CreateAsync(Guid reporterUserId, CreateReportRequest request);

    Task<ReportDetailsResponse> GetByIdAsync(Guid reportId);

    Task<PagedResult<AdminReportListResponse>> GetAllAsync(AdminReportFilterParams filterParams);

    Task<PagedResult<ReportResponse>> GetByReporterAsync(Guid reporterUserId, PaginationParams pagination, SortDirection sortDirection = SortDirection.Desc);

    Task<ReportDetailsResponse> ReviewAsync(Guid reportId, Guid reviewerUserId, ReviewReportRequest request);
    Task<int> CountByStatusAsync(ReportStatus status, DateTime? from, DateTime? to);
    Task<int> CountCurrentlyByStatusAsync(ReportStatus status);

}