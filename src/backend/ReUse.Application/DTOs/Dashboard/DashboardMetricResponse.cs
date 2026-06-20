using ReUse.Application.Enums;

namespace ReUse.Application.DTOs.Dashboard;

public record DashboardMetricResponse
{
    public DashboardMetricType MetricType { get; init; }
    public decimal CurrentValue { get; init; }
    public decimal? PreviousValue { get; init; }
    public double? PercentageChange { get; init; }
}