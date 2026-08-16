using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using RabbitMQ.Client.Events;

namespace Nexus1.Compliance.Infrastructure.Messaging;

/// <summary>
/// Thin hosting loop — mirrors AuditConsumerBackgroundService exactly,
/// including the CONSUMER span/carrier extraction (ADR-011/ADR-013). Queue
/// name and binding routing key are ch.34's frozen shape (Executable Asset
/// 34-U) — an independent binding from the same exchange and routing key
/// Audit is bound to, not a shared queue.
/// </summary>
public sealed class ComplianceConsumerBackgroundService(
    RabbitMqConnectionManager connectionManager,
    RabbitMqOptions rabbitMqOptions,
    ComplianceVerdictMessageHandler messageHandler,
    NexusRuntimeMetrics metrics,
    ILogger<ComplianceConsumerBackgroundService> logger) : BackgroundService
{
    private const string QueueName = "compliance.root-cause-verdicts.v1";
    private const string BindingRoutingKey = "root-cause.root-cause-verdict-issued.v1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = connectionManager.CreateChannel();
        NexusTopology.DeclareQuorumQueue(channel, QueueName, BindingRoutingKey);
        NexusTopology.DeclareDeadQueue(channel, QueueName);

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

                var startedAt = Stopwatch.GetTimestamp();
                MessageHandlingOutcome outcome;
                try
                {
                    outcome = await messageHandler.HandleAsync(messageId, delivery.Body.ToArray(), stoppingToken);
                    RecordProcessAttempt(metrics, startedAt, ToMetricOutcome(outcome), errorType: null);
                }
                catch (Exception ex)
                {
                    SafeError.Record(activity, ex);
                    RecordProcessAttempt(metrics, startedAt, "FAILED", ErrorClassifier.Classify(ex));
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

    private static string ToMetricOutcome(MessageHandlingOutcome outcome) => outcome switch
    {
        MessageHandlingOutcome.Ack => "COMMITTED",
        MessageHandlingOutcome.NackNoRequeue => "REJECTED",
        _ => "ABSTAINED",
    };

    /// <summary>ch.52 52-K's "process" messaging operation — mirrors AlarmFloodConsumerBackgroundService's helper exactly (ADR-014).</summary>
    private static void RecordProcessAttempt(NexusRuntimeMetrics metrics, long startedAt, string outcome, string? errorType)
    {
        var seconds = Stopwatch.GetElapsedTime(startedAt).TotalSeconds;
        if (MetricLabelPolicy.TryFor("process", outcome, NexusActivitySources.Messaging, out var labels))
        {
            var tags = errorType is null ? labels.ToTagList() : (labels with { ErrorType = errorType }).ToTagList();
            metrics.MessageAttempts.Add(1, tags);
            metrics.MessageDuration.Record(seconds, tags);
        }
        else
        {
            metrics.TelemetryRejected.Add(1);
        }
    }
}
