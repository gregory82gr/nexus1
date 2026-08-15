using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Contracts.AlarmManagement;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Testable in isolation from RabbitMQ.Client's delivery types — the
/// BackgroundService just extracts (messageId, envelopeBytes) from a
/// delivery and calls this. Returns true if the caller should ack
/// (success or confirmed duplicate), false if the caller should nack for
/// redelivery (ADR-008).
/// </summary>
public sealed class AlarmFloodMessageHandler(IServiceScopeFactory scopeFactory)
{
    public const string ConsumerName = "rootcause.alarm-events.v1";

    private static readonly JsonSerializerOptions PayloadOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<bool> HandleAsync(Guid messageId, byte[] envelopeBytes, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RootCauseDbContext>();

        // Fast pre-read — cheap early exit for the common duplicate case (ADR-008).
        var alreadyProcessed = await dbContext.InboxReceipts
            .AnyAsync(r => r.ConsumerName == ConsumerName && r.MessageId == messageId, cancellationToken);
        if (alreadyProcessed)
        {
            return true;
        }

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
            ConsumerName, messageId, "alarm-management", "nexus1.alarm-management.alarm-flood-detected.v1",
            schemaVersion: 1, payload.StartedAtUtc, dateTimeProvider.UtcNow);
        dbContext.InboxReceipts.Add(receipt);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
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

            // Confirmed duplicate → ack. Genuinely ambiguous → false, caller nacks.
            return !stillMissing;
        }
    }
}
