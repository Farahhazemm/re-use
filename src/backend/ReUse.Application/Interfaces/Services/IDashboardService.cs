using ReUse.Application.DTOs.Dashboard;
using ReUse.Application.Enums;

namespace ReUse.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<AdminDashboardSummaryResponse> GetSummaryAsync(DashboardPeriod period);
}