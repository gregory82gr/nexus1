using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Contracts.AlarmManagement;
using Nexus1.Contracts.RootCause;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Testable in isolation from RabbitMQ.Client's delivery types — the
/// BackgroundService just extracts (messageId, envelopeBytes) from a
/// delivery and calls this. Returns a MessageHandlingOutcome telling the
/// caller which broker action to take (ADR-009).
/// </summary>
public sealed class AlarmFloodMessageHandler(IServiceScopeFactory scopeFactory, NexusRuntimeMetrics metrics, ILogger<AlarmFloodMessageHandler> logger)
{
    public const string ConsumerName = "rootcause.alarm-events.v1";
    private const string OriginalRoutingKey = "alarm-management.alarm-flood-detected.v1";
    private const string EventType = "nexus1.alarm-management.alarm-flood-detected.v1";
    private const string Producer = "alarm-management";
    private const int SchemaVersion = 1;

    /// <summary>Same coordinates OpenAnalysisCommandHandler uses (ADR-012) — this is the auto-open production path, which inlines Open() rather than going through that handler (ADR-008), so it must publish CaseOpened itself.</summary>
    private const string CaseOpenedRoutingKey = "root-cause.root-cause-case-opened.v1";
    private const string CaseOpenedEventType = "nexus1.root-cause.root-cause-case-opened.v1";

    private static readonly JsonSerializerOptions PayloadOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<MessageHandlingOutcome> HandleAsync(Guid messageId, byte[] envelopeBytes, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RootCauseDbContext>();

        // Fast pre-read — cheap early exit for the common duplicate case (ADR-008).
        var alreadyProcessed = await dbContext.InboxReceipts
            .AnyAsync(r => r.ConsumerName == ConsumerName && r.MessageId == messageId, cancellationToken);
        if (alreadyProcessed)
        {
            RecordInboxOutcome("DUPLICATE_MATCH");
            return MessageHandlingOutcome.Ack;
        }

        try
        {
            var envelope = JsonDocument.Parse(envelopeBytes);
            var payloadJson = envelope.RootElement.GetProperty("payload").GetRawText();
            var payload = JsonSerializer.Deserialize<AlarmFloodDetectedV1>(payloadJson, PayloadOptions)
                ?? throw new InvalidOperationException("AlarmFloodDetectedV1 payload deserialized to null.");

            var idGenerator = scope.ServiceProvider.GetRequiredService<IIdGenerator>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
            var outboxWriter = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();

            // Nested owner span, same shape as OpenAnalysisCommandHandler's —
            // this is the same "root-cause case open" business operation,
            // reached via the auto-open path instead of the manual command.
            using var openActivity = NexusActivitySources.RootCauseSource.StartActivity(
                SpanNames.RootCauseCaseOpen, ActivityKind.Internal, parentContext: default,
                tags: SafeTags.ForOwnerOperation(messageId, "ATTEMPTED"));

            var analysis = RootCauseAnalysis.Open(
                new RootCauseAnalysisId(idGenerator.NextLong()), new UnitId(payload.UnitId), new AlarmFloodId(payload.AlarmFloodId),
                "system:alarm-flood-consumer", dateTimeProvider.UtcNow, payload.StartedAtUtc);
            await dbContext.RootCauseAnalyses.AddAsync(analysis, cancellationToken);

            // Same transaction as the analysis' own commit — this is the
            // real auto-open production path, which inlines Open() rather
            // than going through OpenAnalysisCommandHandler (ADR-008), so it
            // must publish CaseOpened itself for Reporting's projection to
            // see every opened case, not just manually-opened ones (ADR-012).
            outboxWriter.Enqueue(
                CaseOpenedEventType, schemaVersion: 1, CaseOpenedRoutingKey, analysis.OpenedAtUtc,
                new RootCauseCaseOpenedV1(analysis.Id.Value, analysis.UnitId.Value, analysis.AlarmFloodId.Value, analysis.OpenedAtUtc));

            openActivity?.SetTag("nexus1.outcome.code", "COMMITTED");

            var receipt = new InboxReceipt(
                ConsumerName, messageId, Producer, EventType, SchemaVersion, payload.StartedAtUtc, dateTimeProvider.UtcNow);
            dbContext.InboxReceipts.Add(receipt);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                RecordInboxOutcome("COMMITTED");
                return MessageHandlingOutcome.Ack;
            }
            catch (DbUpdateException)
            {
                // A concurrent delivery may have committed the receipt first —
                // re-resolve with a fresh DbContext rather than continuing with
                // this transaction's tracked (losing) objects (ADR-008).
                using var freshScope = scopeFactory.CreateScope();
                var freshDbContext = freshScope.ServiceProvider.GetRequiredService<RootCauseDbContext>();
                var stillMissing = !await freshDbContext.InboxReceipts
                    .AnyAsync(r => r.ConsumerName == ConsumerName && r.MessageId == messageId, cancellationToken);

                // Confirmed duplicate -> ack. Genuinely ambiguous -> the book's
                // "stop without acknowledgement" case (ch.29 29-A) — closest
                // available action with this client is requeue, not a
                // classified retry/poison decision.
                RecordInboxOutcome(stillMissing ? "ABSTAINED" : "DUPLICATE_MATCH");
                return stillMissing ? MessageHandlingOutcome.NackRequeue : MessageHandlingOutcome.Ack;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            RecordInboxOutcome("FAILED", ErrorClassifier.Classify(ex));
            return await RecordFailureAsync(messageId, envelopeBytes, ex, cancellationToken);
        }
    }

    /// <summary>ch.52 52-O's "one terminal observation" rule — exactly one InboxOutcomes measurement per admission decision, never both a duplicate and a failed count for the same delivery.</summary>
    private void RecordInboxOutcome(string outcome, string? errorType = null)
    {
        if (MetricLabelPolicy.TryFor("process", outcome, NexusActivitySources.RootCause, out var labels))
        {
            var tags = errorType is null ? labels.ToTagList() : (labels with { ErrorType = errorType }).ToTagList();
            metrics.InboxOutcomes.Add(1, tags);
        }
        else
        {
            metrics.TelemetryRejected.Add(1);
        }
    }

    private async Task<MessageHandlingOutcome> RecordFailureAsync(
        Guid messageId, byte[] envelopeBytes, Exception failure, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RootCauseDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var nowUtc = dateTimeProvider.UtcNow;

        var priorTickets = await dbContext.RetryTickets
            .Where(t => t.ConsumerName == ConsumerName && t.MessageId == messageId)
            .OrderBy(t => t.Attempt)
            .ToListAsync(cancellationToken);

        var currentAttempt = priorTickets.Count == 0 ? 0 : priorTickets[^1].Attempt;
        var firstFailedAtUtc = priorTickets.Count == 0 ? nowUtc : priorTickets[0].FirstFailedAtUtc;

        var decision = RetryBudget.Evaluate(RetryPolicies.AlarmRead, currentAttempt, firstFailedAtUtc, nowUtc);
        var envelopeSha256 = SHA256.HashData(envelopeBytes);

        if (decision.CanRetry)
        {
            var dueAtUtc = nowUtc + RetryBackoff.EqualJitter(RetryPolicies.AlarmRead, messageId, decision.NextAttempt);
            var ticket = new RetryTicket(
                Guid.NewGuid(), ConsumerName, messageId, decision.NextAttempt, RetryPolicies.AlarmRead.PolicyId,
                failure.GetType().Name, firstFailedAtUtc, dueAtUtc, Producer, EventType, SchemaVersion,
                OriginalRoutingKey, envelopeBytes, envelopeSha256, nowUtc);
            dbContext.RetryTickets.Add(ticket);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                failure, "Transient failure processing {MessageId}; retry {Attempt}/{MaxAttempts} scheduled for {DueAtUtc}.",
                messageId, decision.NextAttempt, RetryPolicies.AlarmRead.MaxRetryAttempts, dueAtUtc);
            return MessageHandlingOutcome.Ack;
        }

        var poison = new PoisonMessage(
            Guid.NewGuid(), ConsumerName, messageId, envelopeSha256, EventType, SchemaVersion,
            decision.Reason, currentAttempt, firstFailedAtUtc, nowUtc);
        dbContext.PoisonMessages.Add(poison);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogError(
            failure, "Retry budget exhausted for {MessageId} ({Reason}) after {Attempts} attempts; quarantining to dead-letter.",
            messageId, decision.Reason, currentAttempt);
        return MessageHandlingOutcome.NackNoRequeue;
    }
}
