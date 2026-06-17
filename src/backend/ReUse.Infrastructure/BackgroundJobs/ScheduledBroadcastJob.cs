using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;

namespace ReUse.Infrastructure.BackgroundJobs;

public class ScheduledBroadcastJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledBroadcastJob> _logger;
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

    public ScheduledBroadcastJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledBroadcastJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TickInterval, stoppingToken);

            try
            {
                await DispatchDueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScheduledBroadcastJob tick failed");
            }
        }
    }

    private async Task DispatchDueAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var broadcastService = scope.ServiceProvider.GetRequiredService<IAdminBroadcastService>();

        var due = await unitOfWork.Broadcasts.GetDueScheduledAsync(DateTime.UtcNow);

        foreach (var broadcast in due)
        {
            if (ct.IsCancellationRequested) break;

            _logger.LogInformation(
                "ScheduledBroadcastJob: dispatching broadcast {BroadcastId}", broadcast.Id);

            try
            {
                await broadcastService.ExecuteAsync(broadcast.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ScheduledBroadcastJob: broadcast {BroadcastId} threw unhandled exception",
                    broadcast.Id);
            }
        }
    }
}