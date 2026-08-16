using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Owns I/O, timeout and last-good semantics for the outbox gauges (ch.52
/// 52-I) — the collection callback itself (<see cref="OutboxMetricState"/>)
/// never touches a database. On a timed-out or failed read, the prior
/// snapshot is simply left in place (no Publish call), so the gauges show a
/// stale-but-honest last-good value rather than resetting to zero.
/// </summary>
public sealed class OutboxMetricRefreshBackgroundService(
    RootCauseOutboxMetricSnapshotReader reader, OutboxMetricState state, ILogger<OutboxMetricRefreshBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var budget = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            budget.CancelAfter(ReadTimeout);

            try
            {
                var snapshot = await reader.ReadAsync(budget.Token);
                state.Publish(NexusActivitySources.RootCause, snapshot);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Outbox metric snapshot refresh failed; retaining last-good gauge values.");
            }

            try
            {
                await Task.Delay(RefreshInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
