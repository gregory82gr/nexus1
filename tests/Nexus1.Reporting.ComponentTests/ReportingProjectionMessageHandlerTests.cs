using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Contracts.RootCause;
using Nexus1.Reporting.Domain;
using Nexus1.Reporting.Infrastructure.Messaging;
using Nexus1.Reporting.Infrastructure.Persistence;

namespace Nexus1.Reporting.ComponentTests;

/// <summary>
/// Proves the two-reducer projection, the AnalysisId-keyed out-of-order
/// pending buffer, dedup on both event types, retry/poison classification,
/// and the explicit unsupported-contract quarantine (ch.35, ADR-012). Real
/// LocalDB, no mocks.
/// </summary>
public sealed class ReportingProjectionMessageHandlerTests : ReportingComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ReportingDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private ReportingProjectionMessageHandler BuildHandler() =>
        new(BuildScopeFactory(), NewMetrics(), NullLogger<ReportingProjectionMessageHandler>.Instance);

    private static byte[] BuildOpenedEnvelope(Guid messageId, long analysisId)
    {
        var payload = new RootCauseCaseOpenedV1(analysisId, 1, 500, NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-case-opened.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    private static byte[] BuildVerdictEnvelope(Guid messageId, long analysisId, string verdict = "Loose fitting confirmed as cause.")
    {
        var payload = new RootCauseVerdictIssuedV1(analysisId, 1, 500, verdict, NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-verdict-issued.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    private static byte[] BuildUnsupportedEnvelope(Guid messageId)
    {
        var payload = new { Note = "not a RootCause fact this projection understands" };
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-case-closed.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    private static byte[] MalformedEnvelope() => "this is not json"u8.ToArray();

    [Fact]
    public async Task In_order_delivery_creates_then_advances_the_same_row()
    {
        var handler = BuildHandler();
        var openedMessageId = Guid.NewGuid();
        var verdictMessageId = Guid.NewGuid();

        var openedOutcome = await handler.HandleAsync(openedMessageId, BuildOpenedEnvelope(openedMessageId, analysisId: 700), CancellationToken.None);
        var verdictOutcome = await handler.HandleAsync(verdictMessageId, BuildVerdictEnvelope(verdictMessageId, analysisId: 700), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, openedOutcome);
        Assert.Equal(MessageHandlingOutcome.Ack, verdictOutcome);

        await using var verifyContext = CreateDbContext();
        var summary = await verifyContext.CaseSummaries.SingleAsync();
        Assert.Equal(700, summary.Id.Value);
        Assert.Equal(ReportingCaseStatus.VerdictIssued, summary.Status);
        Assert.Equal("Loose fitting confirmed as cause.", summary.Verdict);
        Assert.Equal(0, await verifyContext.PendingVerdicts.CountAsync());
    }

    [Fact]
    public async Task Out_of_order_delivery_buffers_the_verdict_then_applies_it_once_the_case_opens()
    {
        var handler = BuildHandler();
        var verdictMessageId = Guid.NewGuid();
        var openedMessageId = Guid.NewGuid();

        // Verdict arrives BEFORE its case-opened event.
        var verdictOutcome = await handler.HandleAsync(verdictMessageId, BuildVerdictEnvelope(verdictMessageId, analysisId: 701), CancellationToken.None);
        Assert.Equal(MessageHandlingOutcome.Ack, verdictOutcome);

        await using (var midContext = CreateDbContext())
        {
            Assert.Equal(0, await midContext.CaseSummaries.CountAsync());
            var pending = await midContext.PendingVerdicts.SingleAsync();
            Assert.Equal(701, pending.AnalysisId);
        }

        var openedOutcome = await handler.HandleAsync(openedMessageId, BuildOpenedEnvelope(openedMessageId, analysisId: 701), CancellationToken.None);
        Assert.Equal(MessageHandlingOutcome.Ack, openedOutcome);

        await using var verifyContext = CreateDbContext();
        var summary = await verifyContext.CaseSummaries.SingleAsync();
        Assert.Equal(ReportingCaseStatus.VerdictIssued, summary.Status);
        Assert.Equal("Loose fitting confirmed as cause.", summary.Verdict);
        Assert.Equal(0, await verifyContext.PendingVerdicts.CountAsync());
    }

    [Fact]
    public async Task Duplicate_CaseOpened_delivery_does_not_recreate_the_row()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();
        var envelopeBytes = BuildOpenedEnvelope(messageId, analysisId: 700);

        await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);
        var secondOutcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, secondOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.CaseSummaries.CountAsync());
        Assert.Equal(1, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task Duplicate_VerdictIssued_delivery_does_not_reapply()
    {
        var handler = BuildHandler();
        var openedMessageId = Guid.NewGuid();
        await handler.HandleAsync(openedMessageId, BuildOpenedEnvelope(openedMessageId, analysisId: 700), CancellationToken.None);

        var verdictMessageId = Guid.NewGuid();
        var verdictEnvelope = BuildVerdictEnvelope(verdictMessageId, analysisId: 700);
        await handler.HandleAsync(verdictMessageId, verdictEnvelope, CancellationToken.None);
        var secondOutcome = await handler.HandleAsync(verdictMessageId, verdictEnvelope, CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, secondOutcome);

        await using var verifyContext = CreateDbContext();
        var summary = await verifyContext.CaseSummaries.SingleAsync();
        Assert.Equal(ReportingCaseStatus.VerdictIssued, summary.Status);
        Assert.Equal(2, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task An_unsupported_event_type_is_quarantined_immediately_without_spending_retry_budget()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        var outcome = await handler.HandleAsync(messageId, BuildUnsupportedEnvelope(messageId), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.NackNoRequeue, outcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.RetryTickets.CountAsync());
        var poison = await verifyContext.PoisonMessages.SingleAsync();
        Assert.Equal("unsupported-contract", poison.TerminalReason);
        Assert.Equal("nexus1.root-cause.root-cause-case-closed.v1", poison.EventType);
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
        Assert.Equal(ReportingProjectionMessageHandler.ConsumerName, ticket.ConsumerName);
        Assert.Equal(1, ticket.Attempt);
        Assert.Equal(0, await verifyContext.PoisonMessages.CountAsync());
    }

    [Fact]
    public async Task Exhausting_the_retry_budget_quarantines_the_message_and_nacks_without_requeue()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        MessageHandlingOutcome lastOutcome = default;
        for (var i = 0; i < RetryPolicies.ReportProject.MaxRetryAttempts + 1; i++)
        {
            lastOutcome = await handler.HandleAsync(messageId, MalformedEnvelope(), CancellationToken.None);
        }

        Assert.Equal(MessageHandlingOutcome.NackNoRequeue, lastOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(RetryPolicies.ReportProject.MaxRetryAttempts, await verifyContext.RetryTickets.CountAsync());

        var poison = await verifyContext.PoisonMessages.SingleAsync();
        Assert.Equal("attempt-budget-exhausted", poison.TerminalReason);
    }
}
