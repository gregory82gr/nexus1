using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using RabbitMQ.Client.Events;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Thin hosting loop — the actual dedup/retry/poison-classification logic
/// lives in AlarmFloodMessageHandler, testable without a live RabbitMQ
/// delivery. Extracts the propagated trace carrier and wraps the handler
/// call in a CONSUMER span (ch.51 51-L) — a malformed or absent carrier
/// starts a new diagnostic root here, it never rewrites the accepted
/// envelope's business identity (ch.51 "INDEPENDENCE RULE").
/// </summary>
public sealed class AlarmFloodConsumerBackgroundService(
    RabbitMqConnectionManager connectionManager,
    RabbitMqOptions rabbitMqOptions,
    AlarmFloodMessageHandler messageHandler,
    ILogger<AlarmFloodConsumerBackgroundService> logger) : BackgroundService
{
    private const string QueueName = "rootcause.alarm-events.v1";
    private const string BindingRoutingKey = "alarm-management.alarm-flood-detected.v1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = connectionManager.CreateChannel();
        NexusTopology.DeclareQuorumQueue(channel, QueueName, BindingRoutingKey);
        NexusTopology.DeclareDeadQueue(channel, QueueName);

        // Dead-lettering is broker policy, not a queue argument (Executable
        // Asset 25-C, ADR-009) — applied once at consumer startup, idempotent
        // (a PUT to an unchanged policy is a no-op).
        await new RabbitMqDeadLetterPolicyProvisioner(rabbitMqOptions)
            .EnsureAsync("nexus-live-queue-safety-" + QueueName, QueueName, stoppingToken);

        channel.BasicQos(0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, delivery) =>
        {
            try
            {
                if (!Guid.TryParse(delivery.BasicProperties.MessageId, out var messageId))
                {
                    throw new InvalidOperationException($"Message has no valid MessageId property: '{delivery.BasicProperties.MessageId}'.");
                }

                var eventType = delivery.BasicProperties.Type ?? "unknown";
                var parentContext = AmqpCarrier.Extract(delivery.BasicProperties.Headers);
                using var activity = NexusActivitySources.MessagingSource.StartActivity(
                    SpanNames.ForProcess(eventType),
                    ActivityKind.Consumer,
                    parentContext,
                    tags: SafeTags.ForMessageProcess(messageId, eventType));

                MessageHandlingOutcome outcome;
                try
                {
                    outcome = await messageHandler.HandleAsync(messageId, delivery.Body.ToArray(), stoppingToken);
                }
                catch (Exception ex)
                {
                    SafeError.Record(activity, ex);
                    throw;
                }
                switch (outcome)
                {
                    case MessageHandlingOutcome.Ack:
                        channel.BasicAck(delivery.DeliveryTag, multiple: false);
                        break;
                    case MessageHandlingOutcome.NackNoRequeue:
                        logger.LogError(
                            "Message {MessageId} quarantined after exhausting retries; routing to dead-letter.",
                            delivery.BasicProperties.MessageId);
                        channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: false);
                        break;
                    case MessageHandlingOutcome.NackRequeue:
                    default:
                        logger.LogWarning("Ambiguous inbox outcome for message {MessageId}; nacking for redelivery.", delivery.BasicProperties.MessageId);
                        channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message {MessageId}; nacking for redelivery.", delivery.BasicProperties.MessageId);
                channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(
            queue: QueueName, autoAck: false, consumerTag: string.Empty, noLocal: false,
            exclusive: false, arguments: null, consumer: consumer);

        var stopped = new TaskCompletionSource();
        await using (stoppingToken.Register(() => stopped.TrySetResult()))
        {
            await stopped.Task;
        }
    }
}
