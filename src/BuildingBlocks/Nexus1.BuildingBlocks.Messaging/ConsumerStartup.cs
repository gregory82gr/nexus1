using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// The shared consumer start sequence with retry (ADR-044). Every consumer BackgroundService used
/// to open its channel, declare its queues, PUT its dead-letter policy and start consuming at the
/// top of ExecuteAsync with no retry — so a broker that was down (or a management API that was
/// not yet up) threw out of ExecuteAsync, and .NET 8's default StopHost behaviour killed the
/// whole host. Here the sequence waits for the connection, retries every failure with backoff,
/// disposes a half-started channel before retrying, and only gives up when the host shuts down.
/// </summary>
public static class ConsumerStartup
{
    /// <summary>
    /// Waits for the broker, then runs <paramref name="startConsuming"/> on a fresh channel until
    /// it succeeds; returns the channel that is now consuming, or null if the host is stopping.
    /// </summary>
    public static Task<IModel?> RunWithRetryAsync(
        RabbitMqConnectionManager connectionManager,
        string consumerName,
        Func<IModel, CancellationToken, Task> startConsuming,
        ILogger logger,
        CancellationToken stoppingToken) =>
        RunWithRetryAsync<IModel>(
            connectionManager.WaitUntilConnectedAsync,
            connectionManager.CreateChannel,
            consumerName,
            startConsuming,
            logger,
            attempt => RetryBackoff.ExponentialCap(RabbitMqConnectionManager.StartupBackoff, attempt),
            stoppingToken);

    /// <summary>The seam the unit tests drive: channel source, start sequence and backoff are all injectable.</summary>
    public static async Task<TChannel?> RunWithRetryAsync<TChannel>(
        Func<CancellationToken, Task> waitUntilConnected,
        Func<TChannel> openChannel,
        string consumerName,
        Func<TChannel, CancellationToken, Task> startConsuming,
        ILogger logger,
        Func<int, TimeSpan> backoff,
        CancellationToken stoppingToken)
        where TChannel : class, IDisposable
    {
        try
        {
            await waitUntilConnected(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return null;
        }

        for (var attempt = 1; !stoppingToken.IsCancellationRequested; attempt++)
        {
            TChannel? channel = null;
            try
            {
                channel = openChannel();
                await startConsuming(channel, stoppingToken);
                logger.LogInformation("Consumer {Consumer} started on attempt {Attempt}.", consumerName, attempt);
                return channel;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                channel?.Dispose();
                return null;
            }
            catch (Exception ex)
            {
                channel?.Dispose();
                var delay = backoff(attempt);
                if (RabbitMqFailure.IsAuthenticationFailure(ex))
                {
                    logger.LogError(ex, RabbitMqFailure.AuthenticationFailureMessage + " Consumer {Consumer} start attempt {Attempt}; retrying in {Delay}.", consumerName, attempt, delay);
                }
                else
                {
                    logger.LogWarning(ex, "Consumer {Consumer} could not start (attempt {Attempt}); retrying in {Delay}.", consumerName, attempt, delay);
                }

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
        }

        return null;
    }
}
