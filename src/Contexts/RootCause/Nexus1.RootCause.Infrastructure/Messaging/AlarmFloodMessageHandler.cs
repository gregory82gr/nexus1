using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Contracts.AlarmManagement;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Testable in isolation from RabbitMQ.Client's delivery types — the
/// BackgroundService just extracts (messageId, envelopeBytes) from a
/// delivery and calls this. Returns a MessageHandlingOutcome telling the
/// caller which broker action to take (ADR-009).
/// </summary>
public sealed class AlarmFloodMessageHandler(IServiceScopeFactory scopeFactory, ILogger<AlarmFloodMessageHandler> logger)
{
    public const string ConsumerName = "rootcause.alarm-events.v1";
    private const string OriginalRoutingKey = "alarm-management.alarm-flood-detected.v1";
    private const string EventType = "nexus1.alarm-management.alarm-flood-detected.v1";
    private const string Producer = "alarm-management";
    private const int SchemaVersion = 1;

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

            var analysis = RootCauseAnalysis.Open(
                new RootCauseAnalysisId(idGenerator.NextLong()), new UnitId(payload.UnitId), new AlarmFloodId(payload.AlarmFloodId),
                "system:alarm-flood-consumer", dateTimeProvider.UtcNow);
            await dbContext.RootCauseAnalyses.AddAsync(analysis, cancellationToken);

            var receipt = new InboxReceipt(
                ConsumerName, messageId, Producer, EventType, SchemaVersion, payload.StartedAtUtc, dateTimeProvider.UtcNow);
            dbContext.InboxReceipts.Add(receipt);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
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
