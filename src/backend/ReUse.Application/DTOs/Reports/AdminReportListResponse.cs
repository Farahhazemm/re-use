using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Reports;

public record AdminReportListResponse
{
    public Guid Id { get; init; }
    public ReportTargetType TargetType { get; init; }
    public Guid TargetId { get; init; }
    public ReportReason Reason { get; init; }
    public ReportStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public ReportUserResponse? Reporter { get; init; }
    public string? ReporterName { get; init; }
    public string? ReporterEmail { get; init; }
    public ReportUserResponse? ReviewedBy { get; init; }
    public DateTime? ReviewedAt { get; init; }
}