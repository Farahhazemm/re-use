using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Application.Enums;

namespace ReUse.Application.DTOs.Dashboard;

public record AdminDashboardSummaryResponse
{
    public DashboardPeriod Period { get; init; }
    public DateTime CurrentPeriodStart { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public DashboardMetricResponse TotalUsers { get; init; } = default!;
    public DashboardMetricResponse ActiveProducts { get; init; } = default!;
    public DashboardMetricResponse TotalRevenue { get; init; } = default!;
    public DashboardMetricResponse PendingDisputes { get; init; } = default!;
}