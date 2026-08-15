using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Mirrors Nexus1.AlarmManagement.Infrastructure.Messaging.
/// OutboxPublisherBackgroundService exactly (ADR-008/ADR-010).
/// </summary>
public sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
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
                var relay = scope.ServiceProvider.GetRequiredService<OutboxRelay>();
                var published = await relay.RelayOnceAsync(BatchSize, stoppingToken);

                if (published == 0)
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
                logger.LogError(ex, "Outbox relay pass failed unexpectedly.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }
}
