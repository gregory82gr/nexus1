using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus1.Audit.Domain;
using Nexus1.Audit.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Contracts.RootCause;

namespace Nexus1.Audit.Infrastructure.Messaging;

/// <summary>
/// Testable in isolation from RabbitMQ.Client's delivery types, mirroring
/// AlarmFloodMessageHandler's shape (ADR-010). Two-key dedup (ch.34 34-AI):
/// a known MessageId short-circuits to Ack (transport truth); an unknown
/// MessageId but already-recorded SourceAnalysisId still records the new
/// inbox receipt but skips the AuditEvidenceRecord insert (semantic truth) —
/// a replay under a new MessageId for the same verdict must not double-audit.
/// </summary>
public sealed class AuditVerdictMessageHandler(IServiceScopeFactory scopeFactory, ILogger<AuditVerdictMessageHandler> logger)
{
    public const string ConsumerName = "audit.root-cause-verdicts.v1";
    private const string Producer = "root-cause";

    private static readonly JsonSerializerOptions PayloadOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<MessageHandlingOutcome> HandleAsync(Guid messageId, byte[] envelopeBytes, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        // Fast pre-read — cheap early exit for the common duplicate case (ADR-008).
        var alreadyProcessed = await dbContext.InboxReceipts
            .AnyAsync(r => r.ConsumerName == ConsumerName && r.MessageId == messageId, cancellationToken);
        if (alreadyProcessed)
        {
            return MessageHandlingOutcome.Ack;
        }

        try
        {
            var envelope = JsonDocument.Parse(envelopeBytes);
            var eventType = envelope.RootElement.GetProperty("eventType").GetString()
                ?? throw new InvalidOperationException("Envelope has no eventType.");
            var schemaVersion = envelope.RootElement.GetProperty("schemaVersion").GetInt32();
            var correlationId = envelope.RootElement.GetProperty("correlationId").GetGuid();
            var causationId = envelope.RootElement.TryGetProperty("causationId", out var causationElement) && causationElement.ValueKind != JsonValueKind.Null
                ? causationElement.GetGuid()
                : (Guid?)null;
            var payloadJson = envelope.RootElement.GetProperty("payload").GetRawText();
            var payload = JsonSerializer.Deserialize<RootCauseVerdictIssuedV1>(payloadJson, PayloadOptions)
                ?? throw new InvalidOperationException("RootCauseVerdictIssuedV1 payload deserialized to null.");

            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
            var nowUtc = dateTimeProvider.UtcNow;

            // Nested owner span — the actual business operation this
            // consumer exists to perform, distinct from the CONSUMER
            // transport span the background service already wraps
            // HandleAsync in.
            using var activity = NexusActivitySources.AuditSource.StartActivity(
                SpanNames.AuditEvidenceRecord, ActivityKind.Internal, parentContext: default,
                tags: SafeTags.ForOwnerOperation(messageId, "ATTEMPTED"));

            bool alreadyAudited;
            try
            {
                // Semantic half of the two-key oracle: this verdict may
                // already be audited under a different (replayed) MessageId.
                alreadyAudited = await dbContext.Evidence
                    .AnyAsync(e => e.SourceAnalysisId == payload.AnalysisId, cancellationToken);

                if (!alreadyAudited)
                {
                    var evidence = AuditEvidenceRecord.Append(
                        new AuditEvidenceId(Guid.NewGuid()), messageId, payload.AnalysisId, eventType, schemaVersion,
                        envelopeBytes, SHA256.HashData(envelopeBytes), correlationId, causationId, payload.IssuedAtUtc, nowUtc);
                    await dbContext.Evidence.AddAsync(evidence, cancellationToken);
                }

                var receipt = new InboxReceipt(ConsumerName, messageId, Producer, eventType, schemaVersion, payload.IssuedAtUtc, nowUtc);
                dbContext.InboxReceipts.Add(receipt);
            }
            catch (Exception ex)
            {
                SafeError.Record(activity, ex);
                throw;
            }

            activity?.SetTag("nexus1.outcome.code", alreadyAudited ? "DUPLICATE_MATCH" : "COMMITTED");

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return MessageHandlingOutcome.Ack;
            }
            catch (DbUpdateException)
            {
                // A concurrent delivery may have committed the receipt (or the
                // evidence row) first — re-resolve with a fresh DbContext
                // rather than continuing with this transaction's tracked
                // (losing) objects (ADR-008).
                using var freshScope = scopeFactory.CreateScope();
                var freshDbContext = freshScope.ServiceProvider.GetRequiredService<AuditDbContext>();
                var stillMissing = !await freshDbContext.InboxReceipts
                    .AnyAsync(r => r.ConsumerName == ConsumerName && r.MessageId == messageId, cancellationToken);

                return stillMissing ? MessageHandlingOutcome.NackRequeue : MessageHandlingOutcome.Ack;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return await RecordFailureAsync(messageId, envelopeBytes, ex, cancellationToken);
        }
    }

    private async Task<MessageHandlingOutcome> RecordFailureAsync(
        Guid messageId, byte[] envelopeBytes, Exception failure, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var nowUtc = dateTimeProvider.UtcNow;

        var priorTickets = await dbContext.RetryTickets
            .Where(t => t.ConsumerName == ConsumerName && t.MessageId == messageId)
            .OrderBy(t => t.Attempt)
            .ToListAsync(cancellationToken);

        var currentAttempt = priorTickets.Count == 0 ? 0 : priorTickets[^1].Attempt;
        var firstFailedAtUtc = priorTickets.Count == 0 ? nowUtc : priorTickets[0].FirstFailedAtUtc;

        var decision = RetryBudget.Evaluate(RetryPolicies.AuditRootCauseVerdicts, currentAttempt, firstFailedAtUtc, nowUtc);
        var envelopeSha256 = SHA256.HashData(envelopeBytes);
        const string eventType = "nexus1.root-cause.root-cause-verdict-issued.v1";
        const string routingKey = "root-cause.root-cause-verdict-issued.v1";

        if (decision.CanRetry)
        {
            var dueAtUtc = nowUtc + RetryBackoff.EqualJitter(RetryPolicies.AuditRootCauseVerdicts, messageId, decision.NextAttempt);
            var ticket = new RetryTicket(
                Guid.NewGuid(), ConsumerName, messageId, decision.NextAttempt, RetryPolicies.AuditRootCauseVerdicts.PolicyId,
                failure.GetType().Name, firstFailedAtUtc, dueAtUtc, Producer, eventType, 1, routingKey, envelopeBytes, envelopeSha256, nowUtc);
            dbContext.RetryTickets.Add(ticket);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                failure, "Transient failure processing {MessageId}; retry {Attempt}/{MaxAttempts} scheduled for {DueAtUtc}.",
                messageId, decision.NextAttempt, RetryPolicies.AuditRootCauseVerdicts.MaxRetryAttempts, dueAtUtc);
            return MessageHandlingOutcome.Ack;
        }

        var poison = new PoisonMessage(
            Guid.NewGuid(), ConsumerName, messageId, envelopeSha256, eventType, 1,
            decision.Reason, currentAttempt, firstFailedAtUtc, nowUtc);
        dbContext.PoisonMessages.Add(poison);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogError(
            failure, "Retry budget exhausted for {MessageId} ({Reason}) after {Attempts} attempts; quarantining to dead-letter.",
            messageId, decision.Reason, currentAttempt);
        return MessageHandlingOutcome.NackNoRequeue;
    }
}
