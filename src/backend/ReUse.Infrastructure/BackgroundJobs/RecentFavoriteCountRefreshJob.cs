using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.BackgroundJobs;

/// <summary>
/// Nightly job that updates the denormalised RecentFavoriteCount column on all
/// active products. Runs at 02:00 UTC. A single bulk UPDATE is used rather than
/// per-row updates to keep the execution time under 5 seconds at 100k products.
/// </summary>
public class RecentFavoriteCountRefreshJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecentFavoriteCountRefreshJob> _logger;

    private static readonly TimeSpan RunAt = TimeSpan.FromHours(2); // 02:00 UTC

    public RecentFavoriteCountRefreshJob(
        IServiceScopeFactory scopeFactory,
        ILogger<RecentFavoriteCountRefreshJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            _logger.LogInformation("RecentFavoriteCountRefreshJob: next run in {Delay}", delay);

            await Task.Delay(delay, stoppingToken);

            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RecentFavoriteCountRefreshJob failed");
            }
        }
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cutoff = DateTime.UtcNow.AddDays(-90);

        var affected = await context.Database.ExecuteSqlRawAsync("""
            UPDATE products p
            SET "RecentFavoriteCount" = (
                SELECT COUNT(*)
                FROM favorites f
                WHERE f."ProductId" = p."Id"
                  AND f."CreatedAt" > {0}
            )
            WHERE p."Status" = 'Active'
            """, cutoff);

        _logger.LogInformation(
            "RecentFavoriteCountRefreshJob: updated {Count} products", affected);
    }

    private static TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.Add(RunAt);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }
}