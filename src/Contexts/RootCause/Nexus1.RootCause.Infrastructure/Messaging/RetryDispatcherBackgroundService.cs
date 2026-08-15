using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Thin hosting loop around RetryDispatcher — same shape and tunables as
/// OutboxPublisherBackgroundService (ADR-009).
/// </summary>
public sealed class RetryDispatcherBackgroundService(
    IServiceScopeFactory scopeFactory, ILogger<RetryDispatcherBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 64;
    private static readonly TimeSpan IdleDelay = TimeSpan.FromMilliseconds(250);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<RetryDispatcher>();
                var dispatched = await dispatcher.DispatchOnceAsync(BatchSize, stoppingToken);

                if (dispatched == 0)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Retry dispatch pass failed unexpectedly.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }
}
