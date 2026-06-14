using ReUse.Domain.Enums;

namespace ReUse.Domain.Entities;

public class Report : BaseEntity
{
    public Guid ReporterUserId { get; set; }
    public User Reporter { get; set; } = default!;

    public ReportTargetType TargetType { get; set; }

    public Guid TargetId { get; set; }

    public ReportReason Reason { get; set; }

    public string? Notes { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNotes { get; set; }
}