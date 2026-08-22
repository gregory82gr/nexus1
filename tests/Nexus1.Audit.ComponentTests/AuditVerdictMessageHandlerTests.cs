using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.Audit.Infrastructure.Messaging;
using Nexus1.Audit.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Contracts.RootCause;

namespace Nexus1.Audit.ComponentTests;

/// <summary>
/// Proves the two-key dedup oracle (ch.34 34-AI), retry/poison
/// classification, and append-only enforcement — real LocalDB, no mocks.
/// </summary>
public sealed class AuditVerdictMessageHandlerTests : AuditComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AuditDbContext>(options => options
            .UseSqlServer(ConnectionString)
            .AddInterceptors(new AuditAppendOnlyInterceptor()));
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private AuditVerdictMessageHandler BuildHandler() =>
        new(BuildScopeFactory(), NewMetrics(), NullLogger<AuditVerdictMessageHandler>.Instance);

    private static byte[] BuildEnvelope(Guid messageId, long analysisId, Guid? correlationId = null)
    {
        var payload = new RootCauseVerdictIssuedV1(analysisId, 1, 500, "Loose fitting confirmed as cause.", NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-verdict-issued.v1", 1, NowUtc,
            "root-cause", correlationId ?? Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    private static byte[] MalformedEnvelope() => "this is not json"u8.ToArray();

    [Fact]
    public async Task First_delivery_records_evidence_and_the_inbox_receipt()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        var outcome = await handler.HandleAsync(messageId, BuildEnvelope(messageId, analysisId: 700), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, outcome);

        await using var verifyContext = CreateDbContext();
        var evidence = await verifyContext.Evidence.SingleAsync();
        Assert.Equal(700, evidence.SourceAnalysisId);
        Assert.Equal(messageId, evidence.SourceMessageId);

        var receipt = await verifyContext.InboxReceipts.SingleAsync();
        Assert.Equal(AuditVerdictMessageHandler.ConsumerName, receipt.ConsumerName);
        Assert.Equal(messageId, receipt.MessageId);
    }

    [Fact]
    public async Task Duplicate_delivery_of_the_same_message_does_not_record_a_second_evidence_row()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();
        var envelopeBytes = BuildEnvelope(messageId, analysisId: 700);

        var firstOutcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);
        var secondOutcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, firstOutcome);
        Assert.Equal(MessageHandlingOutcome.Ack, secondOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.Evidence.CountAsync());
        Assert.Equal(1, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task A_replay_under_a_new_MessageId_for_the_same_verdict_records_a_receipt_but_not_a_second_evidence_row()
    {
        var handler = BuildHandler();
        var firstMessageId = Guid.NewGuid();
        var replayMessageId = Guid.NewGuid();

        // Same AnalysisId (700), different transport MessageId — the
        // semantic half of the two-key oracle (34-AI), not covered by
        // transport dedup alone.
        await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, analysisId: 700), CancellationToken.None);
        var replayOutcome = await handler.HandleAsync(replayMessageId, BuildEnvelope(replayMessageId, analysisId: 700), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, replayOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.Evidence.CountAsync());
        Assert.Equal(2, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task Different_verdicts_are_recorded_independently()
    {
        var handler = BuildHandler();
        var firstMessageId = Guid.NewGuid();
        var secondMessageId = Guid.NewGuid();

        await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, analysisId: 700), CancellationToken.None);
        await handler.HandleAsync(secondMessageId, BuildEnvelope(secondMessageId, analysisId: 701), CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(2, await verifyContext.Evidence.CountAsync());
        Assert.Equal(2, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task A_transient_failure_records_a_retry_ticket_and_still_acks_the_original_delivery()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        var outcome = await handler.HandleAsync(messageId, MalformedEnvelope(), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, outcome);

        await using var verifyContext = CreateDbContext();
        var ticket = await verifyContext.RetryTickets.SingleAsync();
        Assert.Equal(AuditVerdictMessageHandler.ConsumerName, ticket.ConsumerName);
        Assert.Equal(messageId, ticket.MessageId);
        Assert.Equal(1, ticket.Attempt);
        Assert.Null(ticket.PublishedAtUtc);
        Assert.Equal(0, await verifyContext.Evidence.CountAsync());
        Assert.Equal(0, await verifyContext.PoisonMessages.CountAsync());
    }

    [Fact]
    public async Task Exhausting_the_retry_budget_quarantines_the_message_and_nacks_without_requeue()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        MessageHandlingOutcome lastOutcome = default;
        for (var i = 0; i < RetryPolicies.AuditRootCauseVerdicts.MaxRetryAttempts + 1; i++)
        {
            lastOutcome = await handler.HandleAsync(messageId, MalformedEnvelope(), CancellationToken.None);
        }

        Assert.Equal(MessageHandlingOutcome.NackNoRequeue, lastOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(RetryPolicies.AuditRootCauseVerdicts.MaxRetryAttempts, await verifyContext.RetryTickets.CountAsync());

        var poison = await verifyContext.PoisonMessages.SingleAsync();
        Assert.Equal(AuditVerdictMessageHandler.ConsumerName, poison.ConsumerName);
        Assert.Equal("attempt-budget-exhausted", poison.TerminalReason);
        Assert.Equal(RetryPolicies.AuditRootCauseVerdicts.MaxRetryAttempts, poison.RetryAttempts);
    }

    [Fact]
    public async Task Existing_evidence_cannot_be_modified()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();
        await handler.HandleAsync(messageId, BuildEnvelope(messageId, analysisId: 700), CancellationToken.None);

        await using var dbContext = CreateDbContext();
        var evidence = await dbContext.Evidence.SingleAsync();
        dbContext.Entry(evidence).Property(nameof(evidence.SourceAnalysisId)).CurrentValue = 999L;

        await Assert.ThrowsAsync<AuditMutationRejectedException>(() => dbContext.SaveChangesAsync());
    }
}
