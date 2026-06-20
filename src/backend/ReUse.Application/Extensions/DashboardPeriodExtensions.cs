using ReUse.Application.Enums;

namespace ReUse.Application.Extensions;

public record DashboardWindow(
    DateTime CurrentStart,
    DateTime CurrentEnd,
    DateTime PreviousStart,
    DateTime PreviousEnd);

public static class DashboardPeriodExtensions
{
    public static DashboardWindow ToWindow(this DashboardPeriod period, DateTime now)
    {
        now = now.Kind switch
        {
            DateTimeKind.Utc => now,
            DateTimeKind.Local => now.ToUniversalTime(),
            _ => DateTime.SpecifyKind(now, DateTimeKind.Utc)
        };
        var currentEnd = now;

        var currentStart = period switch
        {
            DashboardPeriod.Today => now.Date,
            DashboardPeriod.Last7Days => now.AddDays(-7),
            DashboardPeriod.Last30Days => now.AddDays(-30),
            DashboardPeriod.Last90Days => now.AddDays(-90),
            DashboardPeriod.ThisMonth => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            DashboardPeriod.ThisYear => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };

        var (previousStart, previousEnd) = period switch
        {
            DashboardPeriod.Today => (currentStart.AddDays(-1), currentEnd.AddDays(-1)),
            DashboardPeriod.Last7Days => (currentStart.AddDays(-7), currentEnd.AddDays(-7)),
            DashboardPeriod.Last30Days => (currentStart.AddDays(-30), currentEnd.AddDays(-30)),
            DashboardPeriod.Last90Days => (currentStart.AddDays(-90), currentEnd.AddDays(-90)),
            DashboardPeriod.ThisMonth => (currentStart.AddMonths(-1), currentEnd.AddMonths(-1)),
            DashboardPeriod.ThisYear => (currentStart.AddYears(-1), currentEnd.AddYears(-1)),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };

        return new DashboardWindow(currentStart, currentEnd, previousStart, previousEnd);
    }
}