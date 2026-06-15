using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Reports;

public record ReportResponse
{
    public Guid Id { get; init; }
    public ReportTargetType TargetType { get; init; }
    public Guid TargetId { get; init; }
    public ReportReason Reason { get; init; }
    public string? Notes { get; init; }
    public ReportStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public ReportUserResponse Reporter { get; init; } = default!;
}