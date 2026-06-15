using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Reports;

public record CreateReportRequest(
    ReportTargetType TargetType,
    Guid TargetId,
    ReportReason Reason,
    string? Notes);