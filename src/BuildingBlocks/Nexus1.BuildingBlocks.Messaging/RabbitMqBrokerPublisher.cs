using RabbitMQ.Client;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// AMQP property mapping matches From_Services_To_Runtime's Executable
/// Asset 27-Q exactly, except traceparent/tracestate headers (omitted —
/// no distributed tracing wired up yet, ADR-008).
/// </summary>
public sealed class RabbitMqBrokerPublisher(RabbitMqConnectionManager connectionManager) : IBrokerPublisher
{
    public Task PublishAsync(OutboundMessage message, CancellationToken cancellationToken)
    {
        using var channel = connectionManager.CreateChannel();
        NexusTopology.DeclareEventsExchange(channel);
        channel.ConfirmSelect();

        var properties = channel.CreateBasicProperties();
        properties.AppId = message.Producer;
        properties.MessageId = message.MessageId.ToString("D");
        properties.Type = message.EventType;
        properties.ContentType = "application/json";
        properties.ContentEncoding = "utf-8";
        properties.DeliveryMode = 2; // persistent
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        if (message.CorrelationId.HasValue)
        {
            properties.CorrelationId = message.CorrelationId.Value.ToString("D");
        }

        var headers = new Dictionary<string, object>
        {
            ["schema-version"] = message.SchemaVersion,
            ["envelope-sha256"] = Convert.ToHexString(message.EnvelopeSha256).ToLowerInvariant(),
        };

        if (message.CausationId.HasValue)
        {
            headers["causation-id"] = message.CausationId.Value.ToString("D");
        }

        properties.Headers = headers;

        channel.BasicPublish(NexusTopology.EventsExchange, message.RoutingKey, properties, message.EnvelopeBytes);

        var confirmed = channel.WaitForConfirms(TimeSpan.FromSeconds(10));
        if (!confirmed)
        {
            throw new InvalidOperationException(
                $"RabbitMQ did not confirm publish of message {message.MessageId} within the timeout.");
        }

        return Task.CompletedTask;
    }
}
