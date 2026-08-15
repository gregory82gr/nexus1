using RabbitMQ.Client;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>One connection per host process; channels are created per use (not shared/pooled — RabbitMQ.Client's own recommended pattern).</summary>
public sealed class RabbitMqConnectionManager : IDisposable
{
    private readonly IConnection _connection;

    public RabbitMqConnectionManager(RabbitMqOptions options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            DispatchConsumersAsync = true,
            // Explicit, not relying on the client's implicit default (ADR-009):
            // a broker outage must not take the whole host down with it —
            // CreateChannel() throws while disconnected (caught by OutboxRelay/
            // RetryDispatcher's broad catch, leaving rows unprocessed for
            // redelivery) and the same IConnection reconnects on its own once
            // the broker returns, without restarting the host process.
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
        };

        _connection = factory.CreateConnection("nexus1");
    }

    public IModel CreateChannel() => _connection.CreateModel();

    public void Dispose() => _connection.Dispose();
}
