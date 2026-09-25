using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// One connection per host process; channels are created per use (not shared/pooled — RabbitMQ.Client's own recommended pattern).
///
/// The connection is established in the BACKGROUND, never in the constructor (ADR-044): a broker
/// that is unreachable at startup must not take the host down before it can even serve its
/// health endpoints. A connect loop retries with backoff (1 s doubling to a 30 s cap) until the
/// broker answers. Until then <see cref="CreateChannel"/> throws a clear InvalidOperationException
/// — callers already treat a failed channel as a retryable failure — and consumers wait via
/// <see cref="WaitUntilConnectedAsync"/> (see <see cref="ConsumerStartup"/>). Once connected, the
/// client's own automatic recovery handles later broker outages, exactly as before.
/// </summary>
public sealed class RabbitMqConnectionManager : IDisposable
{
    /// <summary>The connect/consumer-start backoff: 1 s, doubling, capped at 30 s (only the delay fields are read).</summary>
    internal static readonly RetryPolicy StartupBackoff = new(
        PolicyId: "rabbitmq-startup",
        MaxRetryAttempts: 20,
        MaxElapsed: TimeSpan.FromMinutes(10),
        InitialDelay: TimeSpan.FromSeconds(1),
        MaxDelay: TimeSpan.FromSeconds(30),
        EqualJitterPercent: 0);

    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqConnectionManager> _logger;
    private readonly TaskCompletionSource<IConnection> _connected = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource _disposing = new();

    public RabbitMqConnectionManager(RabbitMqOptions options, ILogger<RabbitMqConnectionManager> logger)
    {
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            DispatchConsumersAsync = true,
            // Explicit, not relying on the client's implicit default (ADR-009):
            // a broker outage AFTER the first connect must not take the whole host
            // down with it — CreateChannel() throws while disconnected (caught by
            // OutboxRelay/RetryDispatcher's broad catch, leaving rows unprocessed for
            // redelivery) and the same IConnection reconnects on its own once the
            // broker returns, without restarting the host process.
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
        };

        _ = Task.Run(ConnectLoopAsync);
    }

    /// <summary>True once the first connection has been established (later outages are handled by automatic recovery).</summary>
    public bool IsConnected => _connected.Task.IsCompletedSuccessfully;

    /// <summary>Completes when the first connection is established; cancellable (e.g. on host shutdown).</summary>
    public Task WaitUntilConnectedAsync(CancellationToken cancellationToken) => _connected.Task.WaitAsync(cancellationToken);

    public IModel CreateChannel() => IsConnected
        ? _connected.Task.Result.CreateModel()
        : throw new InvalidOperationException("The RabbitMQ connection has not been established yet; the background connect loop is still retrying.");

    public void Dispose()
    {
        _disposing.Cancel();
        if (IsConnected)
        {
            _connected.Task.Result.Dispose();
        }

        _disposing.Dispose();
    }

    private async Task ConnectLoopAsync()
    {
        for (var attempt = 1; !_disposing.IsCancellationRequested; attempt++)
        {
            try
            {
                var connection = _factory.CreateConnection("nexus1");
                _connected.TrySetResult(connection);
                _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port} on attempt {Attempt}.", _factory.HostName, _factory.Port, attempt);
                return;
            }
            catch (Exception ex) when (!_disposing.IsCancellationRequested)
            {
                var delay = RetryBackoff.ExponentialCap(StartupBackoff, attempt);
                if (RabbitMqFailure.IsAuthenticationFailure(ex))
                {
                    _logger.LogError(ex, RabbitMqFailure.AuthenticationFailureMessage + " Connect attempt {Attempt} to {Host}:{Port}; retrying in {Delay}.", attempt, _factory.HostName, _factory.Port, delay);
                }
                else
                {
                    _logger.LogWarning(ex, "RabbitMQ unreachable at {Host}:{Port} (connect attempt {Attempt}); retrying in {Delay}.", _factory.HostName, _factory.Port, attempt, delay);
                }

                try
                {
                    await Task.Delay(delay, _disposing.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
