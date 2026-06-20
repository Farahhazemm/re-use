using ReUse.Application.DTOs.Dashboard;
using ReUse.Application.DTOs.Products.Responses;
using ReUse.Application.Enums;
using ReUse.Application.Extensions;
using ReUse.Application.Interfaces.Services;
using ReUse.Application.Interfaces.Services.External;
using ReUse.Domain.Enums;

namespace ReUse.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserService _userService;
    private readonly IProductService _productService;
    private readonly IPaymentService _paymentService;
    private readonly IReportService _reportService;

    public DashboardService(
        IUserService userService,
        IProductService productService,
        IPaymentService paymentService,
        IReportService reportService)
    {
        _userService = userService;
        _productService = productService;
        _paymentService = paymentService;
        _reportService = reportService;
    }

    public async Task<AdminDashboardSummaryResponse> GetSummaryAsync(DashboardPeriod period = DashboardPeriod.Today)
    {
        if (!Enum.IsDefined(typeof(DashboardPeriod), period))
            throw new InvalidOperationException("Invalid period value.");

        var window = period.ToWindow(DateTime.UtcNow);

        var productSummary = await _productService.GetAdminSummaryAsync();

        var totalUsers = await BuildUserMetricAsync(window);
        var totalRevenue = await BuildRevenueMetricAsync(window);
        var pendingDisputes = await BuildDisputeMetricAsync();
        var activeProducts = BuildActiveProductsMetric(productSummary);

        return new AdminDashboardSummaryResponse
        {
            Period = period,
            CurrentPeriodStart = window.CurrentStart,
            CurrentPeriodEnd = window.CurrentEnd,
            TotalUsers = totalUsers,
            ActiveProducts = activeProducts,
            TotalRevenue = totalRevenue,
            PendingDisputes = pendingDisputes
        };
    }

    private async Task<DashboardMetricResponse> BuildUserMetricAsync(DashboardWindow window)
    {
        var current = await _userService.CountAsync(null, window.CurrentEnd);
        var previous = await _userService.CountAsync(null, window.PreviousEnd);

        return BuildMetric(DashboardMetricType.Cumulative, current, previous);
    }

    private async Task<DashboardMetricResponse> BuildRevenueMetricAsync(DashboardWindow window)
    {
        var current = await _paymentService.SumSuccessfulAsync(window.CurrentStart, window.CurrentEnd);
        var previous = await _paymentService.SumSuccessfulAsync(window.PreviousStart, window.PreviousEnd);

        return BuildMetric(DashboardMetricType.Period, current, previous);
    }

    private async Task<DashboardMetricResponse> BuildDisputeMetricAsync()
    {
        var current = await _reportService.CountCurrentlyByStatusAsync(ReportStatus.Pending);

        return new DashboardMetricResponse
        {
            MetricType = DashboardMetricType.Snapshot,
            CurrentValue = current,
            PreviousValue = null,
            PercentageChange = null
        };
    }

    private static DashboardMetricResponse BuildActiveProductsMetric(AdminProductsSummaryResponse summary)
    {
        return new DashboardMetricResponse
        {
            MetricType = DashboardMetricType.Snapshot,
            CurrentValue = summary.ActiveCount,
            PreviousValue = null,
            PercentageChange = null
        };
    }

    private static DashboardMetricResponse BuildMetric(DashboardMetricType metricType, decimal current, decimal previous)
    {
        return new DashboardMetricResponse
        {
            MetricType = metricType,
            CurrentValue = current,
            PreviousValue = previous,
            PercentageChange = previous.CalculatePercentageChange(current)
        };
    }
}