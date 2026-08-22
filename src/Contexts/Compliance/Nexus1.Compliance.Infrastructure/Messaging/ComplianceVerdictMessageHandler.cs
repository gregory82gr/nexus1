using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Compliance.Domain;
using Nexus1.Compliance.Infrastructure.Persistence;
using Nexus1.Contracts.RootCause;

namespace Nexus1.Compliance.Infrastructure.Messaging;

/// <summary>
/// Mirrors AuditVerdictMessageHandler's shape (ADR-011). Two-key dedup
/// (ch.34 34-AO): a known MessageId short-circuits to Ack (transport truth);
/// an unknown MessageId but already-recorded SourceAnalysisId still records
/// the new inbox receipt but skips opening a second ComplianceReview
/// (semantic truth) — a replay under a new MessageId for the same verdict
/// must not open a second review.
/// </summary>
public sealed class ComplianceVerdictMessageHandler(IServiceScopeFactory scopeFactory, NexusRuntimeMetrics metrics, ILogger<ComplianceVerdictMessageHandler> logger)
{
    public const string ConsumerName = "compliance.root-cause-verdicts.v1";
    private const string Producer = "root-cause";

    private static readonly JsonSerializerOptions PayloadOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<MessageHandlingOutcome> HandleAsync(Guid messageId, byte[] envelopeBytes, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();

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
            var eventType = envelope.RootElement.GetProperty("eventType").GetString()
                ?? throw new InvalidOperationException("Envelope has no eventType.");
            var schemaVersion = envelope.RootElement.GetProperty("schemaVersion").GetInt32();
            var payloadJson = envelope.RootElement.GetProperty("payload").GetRawText();
            var payload = JsonSerializer.Deserialize<RootCauseVerdictIssuedV1>(payloadJson, PayloadOptions)
                ?? throw new InvalidOperationException("RootCauseVerdictIssuedV1 payload deserialized to null.");

            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
            var nowUtc = dateTimeProvider.UtcNow;

            // Nested owner span — the actual business operation this
            // consumer exists to perform, distinct from the CONSUMER
            // transport span the background service already wraps
            // HandleAsync in.
            using var activity = NexusActivitySources.ComplianceSource.StartActivity(
                SpanNames.ComplianceReviewOpen, ActivityKind.Internal, parentContext: default,
                tags: SafeTags.ForOwnerOperation(messageId, "ATTEMPTED"));

            bool alreadyReviewed;
            try
            {
                // Semantic half of the two-key oracle: this verdict may
                // already have an open review under a different (replayed)
                // MessageId.
                alreadyReviewed = await dbContext.Reviews
                    .AnyAsync(r => r.SourceAnalysisId == payload.AnalysisId, cancellationToken);

                if (!alreadyReviewed)
                {
                    var review = ComplianceReview.Open(
                        new ComplianceReviewId(Guid.NewGuid()), messageId, payload.AnalysisId, payload.Verdict, nowUtc);
                    await dbContext.Reviews.AddAsync(review, cancellationToken);
                }

                var receipt = new InboxReceipt(ConsumerName, messageId, Producer, eventType, schemaVersion, payload.IssuedAtUtc, nowUtc);
                dbContext.InboxReceipts.Add(receipt);
            }
            catch (Exception ex)
            {
                SafeError.Record(activity, ex);
                throw;
            }

            activity?.SetTag("nexus1.outcome.code", alreadyReviewed ? "DUPLICATE_MATCH" : "COMMITTED");

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                RecordInboxOutcome(alreadyReviewed ? "DUPLICATE_MATCH" : "COMMITTED");
                return MessageHandlingOutcome.Ack;
            }
            catch (DbUpdateException)
            {
                using var freshScope = scopeFactory.CreateScope();
                var freshDbContext = freshScope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
                var stillMissing = !await freshDbContext.InboxReceipts
                    .AnyAsync(r => r.ConsumerName == ConsumerName && r.MessageId == messageId, cancellationToken);

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

    /// <summary>ch.52 52-O's "one terminal observation" rule — mirrors AuditVerdictMessageHandler's helper exactly (ADR-014).</summary>
    private void RecordInboxOutcome(string outcome, string? errorType = null)
    {
        if (MetricLabelPolicy.TryFor("process", outcome, NexusActivitySources.Compliance, out var labels))
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
        var dbContext = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var nowUtc = dateTimeProvider.UtcNow;

        var priorTickets = await dbContext.RetryTickets
            .Where(t => t.ConsumerName == ConsumerName && t.MessageId == messageId)
            .OrderBy(t => t.Attempt)
            .ToListAsync(cancellationToken);

        var currentAttempt = priorTickets.Count == 0 ? 0 : priorTickets[^1].Attempt;
        var firstFailedAtUtc = priorTickets.Count == 0 ? nowUtc : priorTickets[0].FirstFailedAtUtc;

        var decision = RetryBudget.Evaluate(RetryPolicies.ComplianceRootCauseVerdicts, currentAttempt, firstFailedAtUtc, nowUtc);
        var envelopeSha256 = SHA256.HashData(envelopeBytes);
        const string eventType = "nexus1.root-cause.root-cause-verdict-issued.v1";
        const string routingKey = "root-cause.root-cause-verdict-issued.v1";

        if (decision.CanRetry)
        {
            var dueAtUtc = nowUtc + RetryBackoff.EqualJitter(RetryPolicies.ComplianceRootCauseVerdicts, messageId, decision.NextAttempt);
            var ticket = new RetryTicket(
                Guid.NewGuid(), ConsumerName, messageId, decision.NextAttempt, RetryPolicies.ComplianceRootCauseVerdicts.PolicyId,
                failure.GetType().Name, firstFailedAtUtc, dueAtUtc, Producer, eventType, 1, routingKey, envelopeBytes, envelopeSha256, nowUtc);
            dbContext.RetryTickets.Add(ticket);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                failure, "Transient failure processing {MessageId}; retry {Attempt}/{MaxAttempts} scheduled for {DueAtUtc}.",
                messageId, decision.NextAttempt, RetryPolicies.ComplianceRootCauseVerdicts.MaxRetryAttempts, dueAtUtc);
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
