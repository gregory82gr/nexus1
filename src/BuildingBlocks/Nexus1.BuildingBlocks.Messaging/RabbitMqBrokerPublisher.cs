using System.Diagnostics;
using Nexus1.BuildingBlocks.Observability;
using RabbitMQ.Client;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// AMQP property mapping matches From_Services_To_Runtime's Executable
/// Asset 27-Q. Wraps the publish attempt in a PRODUCER span (ch.51 51-J) —
/// linked to the caller's optional ProducerTraceSnapshot, never parented to
/// it (delayed dispatch is a new attempt, not a continuation), with the W3C
/// trace carrier injected into AMQP headers for the consumer to extract.
/// Also records the "publish" message.attempts/duration measurement
/// (ch.52 52-K) — every context's outbox dispatch goes through this one
/// class, so this is the transport-level attempt count regardless of which
/// context or retry ticket triggered it.
/// </summary>
public sealed class RabbitMqBrokerPublisher(RabbitMqConnectionManager connectionManager, NexusRuntimeMetrics metrics) : IBrokerPublisher
{
    public Task PublishAsync(OutboundMessage message, CancellationToken cancellationToken)
    {
        var links = message.TraceSnapshot is { } snapshot
            ? new[] { new ActivityLink(snapshot.ToActivityContext()) }
            : Array.Empty<ActivityLink>();

        using var activity = NexusActivitySources.MessagingSource.StartActivity(
            SpanNames.ForPublish(message.EventType),
            ActivityKind.Producer,
            parentContext: default,
            tags: SafeTags.ForMessagePublish(message.MessageId, message.EventType, message.RoutingKey),
            links: links);

        var startedAt = Stopwatch.GetTimestamp();
        var outcome = "COMMITTED";
        string? errorType = null;

        try
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

            AmqpCarrier.Inject(activity?.Context ?? default, headers);
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
        catch (Exception ex)
        {
            SafeError.Record(activity, ex);
            outcome = "FAILED";
            errorType = ErrorClassifier.Classify(ex);
            throw;
        }
        finally
        {
            var seconds = Stopwatch.GetElapsedTime(startedAt).TotalSeconds;
            if (MetricLabelPolicy.TryFor("publish", outcome, NexusActivitySources.Messaging, out var labels))
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
}
