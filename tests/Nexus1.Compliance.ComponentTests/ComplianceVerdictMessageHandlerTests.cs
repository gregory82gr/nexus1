using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Compliance.Domain;
using Nexus1.Compliance.Infrastructure.Messaging;
using Nexus1.Compliance.Infrastructure.Persistence;
using Nexus1.Contracts.RootCause;

namespace Nexus1.Compliance.ComponentTests;

/// <summary>
/// Proves the two-key dedup oracle (ch.34 34-AO), retry/poison
/// classification, and — the deliberate asymmetry with Audit — that a
/// ComplianceReview CAN be mutated without an interceptor rejecting it.
/// Real LocalDB, no mocks.
/// </summary>
public sealed class ComplianceVerdictMessageHandlerTests : ComplianceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ComplianceDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private ComplianceVerdictMessageHandler BuildHandler() =>
        new(BuildScopeFactory(), NewMetrics(), NullLogger<ComplianceVerdictMessageHandler>.Instance);

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
    public async Task First_delivery_opens_a_pending_review_and_records_the_inbox_receipt()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        var outcome = await handler.HandleAsync(messageId, BuildEnvelope(messageId, analysisId: 700), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, outcome);

        await using var verifyContext = CreateDbContext();
        var review = await verifyContext.Reviews.SingleAsync();
        Assert.Equal(700, review.SourceAnalysisId);
        Assert.Equal(messageId, review.SourceMessageId);
        Assert.Equal(ComplianceReviewState.Pending, review.State);
        Assert.Equal("Loose fitting confirmed as cause.", review.Verdict);

        var receipt = await verifyContext.InboxReceipts.SingleAsync();
        Assert.Equal(ComplianceVerdictMessageHandler.ConsumerName, receipt.ConsumerName);
        Assert.Equal(messageId, receipt.MessageId);
    }

    [Fact]
    public async Task Duplicate_delivery_of_the_same_message_does_not_open_a_second_review()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();
        var envelopeBytes = BuildEnvelope(messageId, analysisId: 700);

        var firstOutcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);
        var secondOutcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, firstOutcome);
        Assert.Equal(MessageHandlingOutcome.Ack, secondOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.Reviews.CountAsync());
        Assert.Equal(1, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task A_replay_under_a_new_MessageId_for_the_same_verdict_records_a_receipt_but_not_a_second_review()
    {
        var handler = BuildHandler();
        var firstMessageId = Guid.NewGuid();
        var replayMessageId = Guid.NewGuid();

        await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, analysisId: 700), CancellationToken.None);
        var replayOutcome = await handler.HandleAsync(replayMessageId, BuildEnvelope(replayMessageId, analysisId: 700), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, replayOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.Reviews.CountAsync());
        Assert.Equal(2, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task Different_verdicts_open_independent_reviews()
    {
        var handler = BuildHandler();
        var firstMessageId = Guid.NewGuid();
        var secondMessageId = Guid.NewGuid();

        await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, analysisId: 700), CancellationToken.None);
        await handler.HandleAsync(secondMessageId, BuildEnvelope(secondMessageId, analysisId: 701), CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(2, await verifyContext.Reviews.CountAsync());
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
        Assert.Equal(ComplianceVerdictMessageHandler.ConsumerName, ticket.ConsumerName);
        Assert.Equal(1, ticket.Attempt);
        Assert.Null(ticket.PublishedAtUtc);
        Assert.Equal(0, await verifyContext.Reviews.CountAsync());
        Assert.Equal(0, await verifyContext.PoisonMessages.CountAsync());
    }

    [Fact]
    public async Task Exhausting_the_retry_budget_quarantines_the_message_and_nacks_without_requeue()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        MessageHandlingOutcome lastOutcome = default;
        for (var i = 0; i < RetryPolicies.ComplianceRootCauseVerdicts.MaxRetryAttempts + 1; i++)
        {
            lastOutcome = await handler.HandleAsync(messageId, MalformedEnvelope(), CancellationToken.None);
        }

        Assert.Equal(MessageHandlingOutcome.NackNoRequeue, lastOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(RetryPolicies.ComplianceRootCauseVerdicts.MaxRetryAttempts, await verifyContext.RetryTickets.CountAsync());

        var poison = await verifyContext.PoisonMessages.SingleAsync();
        Assert.Equal(ComplianceVerdictMessageHandler.ConsumerName, poison.ConsumerName);
        Assert.Equal("attempt-budget-exhausted", poison.TerminalReason);
        Assert.Equal(RetryPolicies.ComplianceRootCauseVerdicts.MaxRetryAttempts, poison.RetryAttempts);
    }

    [Fact]
    public async Task Existing_review_state_can_be_changed_unlike_Audit_evidence()
    {
        // Deliberate asymmetry with Audit's Existing_evidence_cannot_be_modified
        // test (ADR-011): no append-only interceptor is registered for
        // ComplianceDbContext, so this must succeed rather than throw.
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();
        await handler.HandleAsync(messageId, BuildEnvelope(messageId, analysisId: 700), CancellationToken.None);

        await using var dbContext = CreateDbContext();
        var review = await dbContext.Reviews.SingleAsync();
        dbContext.Entry(review).Property(nameof(review.SourceAnalysisId)).CurrentValue = 999L;
        await dbContext.SaveChangesAsync();

        await using var verifyContext = CreateDbContext();
        var updated = await verifyContext.Reviews.SingleAsync();
        Assert.Equal(999, updated.SourceAnalysisId);
    }
}
