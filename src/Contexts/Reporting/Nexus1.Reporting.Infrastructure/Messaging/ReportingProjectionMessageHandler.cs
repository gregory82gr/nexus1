using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Contracts.RootCause;
using Nexus1.Reporting.Domain;
using Nexus1.Reporting.Infrastructure.Persistence;
using Nexus1.Reporting.Infrastructure.Projection;

namespace Nexus1.Reporting.Infrastructure.Messaging;

/// <summary>
/// Two reducers over one wildcard-bound queue (ch.35, ADR-012): CaseOpened
/// creates the projection row and resolves any pending verdict that arrived
/// first; VerdictIssued updates an existing row or, if the row doesn't
/// exist yet, buffers into PendingVerdict keyed by AnalysisId — the reduced
/// stand-in for the book's stream-position gap buffer. A wildcard binding
/// is not a wildcard reducer (35's own principle): any event type outside
/// the explicit two-member allowlist is quarantined immediately, no retry
/// spent, since an unsupported contract is a permanent classification, not
/// a transient one.
/// </summary>
public sealed class ReportingProjectionMessageHandler(IServiceScopeFactory scopeFactory, ILogger<ReportingProjectionMessageHandler> logger)
{
    public const string ConsumerName = "reporting.integration-events.v1";
    private const string Producer = "root-cause";

    private const string CaseOpenedEventType = "nexus1.root-cause.root-cause-case-opened.v1";
    private const string VerdictIssuedEventType = "nexus1.root-cause.root-cause-verdict-issued.v1";

    private static readonly JsonSerializerOptions PayloadOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<MessageHandlingOutcome> HandleAsync(Guid messageId, byte[] envelopeBytes, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();

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
            var payloadJson = envelope.RootElement.GetProperty("payload").GetRawText();

            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
            var nowUtc = dateTimeProvider.UtcNow;

            DateTime occurredAtUtc;
            switch (eventType)
            {
                case CaseOpenedEventType:
                    var opened = JsonSerializer.Deserialize<RootCauseCaseOpenedV1>(payloadJson, PayloadOptions)
                        ?? throw new InvalidOperationException("RootCauseCaseOpenedV1 payload deserialized to null.");
                    await ApplyOpenedAsync(dbContext, opened, messageId, nowUtc, cancellationToken);
                    occurredAtUtc = opened.OpenedAtUtc;
                    break;

                case VerdictIssuedEventType:
                    var verdictIssued = JsonSerializer.Deserialize<RootCauseVerdictIssuedV1>(payloadJson, PayloadOptions)
                        ?? throw new InvalidOperationException("RootCauseVerdictIssuedV1 payload deserialized to null.");
                    await ApplyVerdictIssuedAsync(dbContext, verdictIssued, messageId, nowUtc, cancellationToken);
                    occurredAtUtc = verdictIssued.IssuedAtUtc;
                    break;

                default:
                    // A wildcard binding is not a wildcard reducer (ch.35) —
                    // an unrecognized RootCause event type is a permanent
                    // classification, quarantined immediately, no retry spent.
                    return await QuarantineUnsupportedContractAsync(dbContext, messageId, envelopeBytes, eventType, schemaVersion, nowUtc, cancellationToken);
            }

            var receipt = new InboxReceipt(ConsumerName, messageId, Producer, eventType, schemaVersion, occurredAtUtc, nowUtc);
            dbContext.InboxReceipts.Add(receipt);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return MessageHandlingOutcome.Ack;
            }
            catch (DbUpdateException)
            {
                using var freshScope = scopeFactory.CreateScope();
                var freshDbContext = freshScope.ServiceProvider.GetRequiredService<ReportingDbContext>();
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

    private static async Task ApplyOpenedAsync(
        ReportingDbContext dbContext, RootCauseCaseOpenedV1 opened, Guid messageId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var id = new RootCauseCaseSummaryId(opened.AnalysisId);
        var alreadyOpened = await dbContext.CaseSummaries.AnyAsync(s => s.Id == id, cancellationToken);
        if (alreadyOpened)
        {
            return; // Replayed CaseOpened for a case that already exists — idempotent no-op.
        }

        var summary = RootCauseCaseSummary.ApplyOpened(id, opened.UnitId, opened.AlarmFloodId, opened.OpenedAtUtc, nowUtc, messageId);
        await dbContext.CaseSummaries.AddAsync(summary, cancellationToken);

        // The verdict may have arrived first — resolve it now that the row exists.
        var pending = await dbContext.PendingVerdicts.SingleOrDefaultAsync(p => p.AnalysisId == opened.AnalysisId, cancellationToken);
        if (pending is not null)
        {
            summary.ApplyVerdictIssued(pending.Verdict, pending.VerdictIssuedAtUtc, nowUtc, pending.MessageId);
            dbContext.PendingVerdicts.Remove(pending);
        }
    }

    private static async Task ApplyVerdictIssuedAsync(
        ReportingDbContext dbContext, RootCauseVerdictIssuedV1 verdictIssued, Guid messageId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var id = new RootCauseCaseSummaryId(verdictIssued.AnalysisId);
        var summary = await dbContext.CaseSummaries.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (summary is not null)
        {
            if (summary.Status == ReportingCaseStatus.Open)
            {
                summary.ApplyVerdictIssued(verdictIssued.Verdict, verdictIssued.IssuedAtUtc, nowUtc, messageId);
            }
            // Already VerdictIssued — replayed delivery, idempotent no-op.
            return;
        }

        // Out-of-order: the case-opened row doesn't exist yet. Buffer, don't lose it.
        var alreadyPending = await dbContext.PendingVerdicts.AnyAsync(p => p.AnalysisId == verdictIssued.AnalysisId, cancellationToken);
        if (!alreadyPending)
        {
            await dbContext.PendingVerdicts.AddAsync(
                new PendingVerdict(verdictIssued.AnalysisId, messageId, verdictIssued.Verdict, verdictIssued.IssuedAtUtc, nowUtc),
                cancellationToken);
        }
    }

    private async Task<MessageHandlingOutcome> QuarantineUnsupportedContractAsync(
        ReportingDbContext dbContext, Guid messageId, byte[] envelopeBytes, string eventType, int schemaVersion,
        DateTime nowUtc, CancellationToken cancellationToken)
    {
        var envelopeSha256 = SHA256.HashData(envelopeBytes);
        var poison = new PoisonMessage(
            Guid.NewGuid(), ConsumerName, messageId, envelopeSha256, eventType, schemaVersion,
            "unsupported-contract", retryAttempts: 0, firstFailedAtUtc: nowUtc, quarantinedAtUtc: nowUtc);
        dbContext.PoisonMessages.Add(poison);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogError(
            "Message {MessageId} has unsupported eventType {EventType}; quarantining without spending retry budget.",
            messageId, eventType);
        return MessageHandlingOutcome.NackNoRequeue;
    }

    private async Task<MessageHandlingOutcome> RecordFailureAsync(
        Guid messageId, byte[] envelopeBytes, Exception failure, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var nowUtc = dateTimeProvider.UtcNow;

        var priorTickets = await dbContext.RetryTickets
            .Where(t => t.ConsumerName == ConsumerName && t.MessageId == messageId)
            .OrderBy(t => t.Attempt)
            .ToListAsync(cancellationToken);

        var currentAttempt = priorTickets.Count == 0 ? 0 : priorTickets[^1].Attempt;
        var firstFailedAtUtc = priorTickets.Count == 0 ? nowUtc : priorTickets[0].FirstFailedAtUtc;

        var decision = RetryBudget.Evaluate(RetryPolicies.ReportProject, currentAttempt, firstFailedAtUtc, nowUtc);
        var envelopeSha256 = SHA256.HashData(envelopeBytes);

        // Best-effort eventType for the ticket/poison record — if the
        // envelope itself couldn't even be parsed, fall back to "unknown".
        // The routing key a retry republish needs is a real, specific key
        // (never the "root-cause.#" wildcard this queue is *bound* with —
        // that's only valid as a binding pattern, not a publishable routing
        // key), derived from eventType using this project's own convention
        // (routingKey = eventType without the "nexus1." prefix — true for
        // every event type in this project, including both of Reporting's).
        string eventType;
        string routingKey;
        try
        {
            eventType = JsonDocument.Parse(envelopeBytes).RootElement.GetProperty("eventType").GetString() ?? "unknown";
            routingKey = eventType.StartsWith("nexus1.", StringComparison.Ordinal) ? eventType["nexus1.".Length..] : "unknown";
        }
        catch (JsonException)
        {
            eventType = "unknown";
            routingKey = "unknown";
        }

        if (decision.CanRetry)
        {
            var dueAtUtc = nowUtc + RetryBackoff.EqualJitter(RetryPolicies.ReportProject, messageId, decision.NextAttempt);
            var ticket = new RetryTicket(
                Guid.NewGuid(), ConsumerName, messageId, decision.NextAttempt, RetryPolicies.ReportProject.PolicyId,
                failure.GetType().Name, firstFailedAtUtc, dueAtUtc, Producer, eventType, 1,
                routingKey, envelopeBytes, envelopeSha256, nowUtc);
            dbContext.RetryTickets.Add(ticket);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                failure, "Transient failure processing {MessageId}; retry {Attempt}/{MaxAttempts} scheduled for {DueAtUtc}.",
                messageId, decision.NextAttempt, RetryPolicies.ReportProject.MaxRetryAttempts, dueAtUtc);
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
