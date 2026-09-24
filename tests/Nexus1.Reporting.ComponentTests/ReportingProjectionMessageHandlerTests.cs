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

    private static byte[] BuildInconclusiveEnvelope(Guid messageId, long analysisId, string reason = "Records incomplete; provenance trail could not be established.", long? alarmFloodId = 500)
    {
        var payload = new RootCauseCaseInconclusiveV1(analysisId, 1, alarmFloodId, reason, NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-case-inconclusive.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    private static byte[] BuildProvenanceOpenedEnvelope(Guid messageId, long analysisId)
    {
        // A provenance-originated case has no flood (ADR-040): AlarmFloodId null.
        var payload = new RootCauseCaseOpenedV1(analysisId, 1, null, NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.root-cause.root-cause-case-opened.v1", 1, NowUtc, "root-cause", Guid.NewGuid(), null, payload);
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
    public async Task In_order_inconclusive_delivery_projects_the_inconclusive_status_with_its_reason()
    {
        var handler = BuildHandler();
        var openedMessageId = Guid.NewGuid();
        var inconclusiveMessageId = Guid.NewGuid();

        var openedOutcome = await handler.HandleAsync(openedMessageId, BuildOpenedEnvelope(openedMessageId, analysisId: 800), CancellationToken.None);
        var inconclusiveOutcome = await handler.HandleAsync(inconclusiveMessageId, BuildInconclusiveEnvelope(inconclusiveMessageId, analysisId: 800), CancellationToken.None);

        // Acked, NOT quarantined -- proves the allowlist entry prevents the
        // poison-on-wildcard-delivery regression (the event matches root-cause.#).
        Assert.Equal(MessageHandlingOutcome.Ack, openedOutcome);
        Assert.Equal(MessageHandlingOutcome.Ack, inconclusiveOutcome);

        await using var verifyContext = CreateDbContext();
        var summary = await verifyContext.CaseSummaries.SingleAsync();
        Assert.Equal(ReportingCaseStatus.Inconclusive, summary.Status);
        Assert.Equal("Records incomplete; provenance trail could not be established.", summary.Reason);
        Assert.Null(summary.Verdict);
        Assert.Equal(0, await verifyContext.PoisonMessages.CountAsync());
    }

    [Fact]
    public async Task A_provenance_originated_case_projects_inconclusive_with_a_null_alarm_flood_id()
    {
        var handler = BuildHandler();
        var openedMessageId = Guid.NewGuid();
        var inconclusiveMessageId = Guid.NewGuid();

        // The EVT-2026-0420 shape: opened with no flood, then inconclusive with no flood.
        await handler.HandleAsync(openedMessageId, BuildProvenanceOpenedEnvelope(openedMessageId, analysisId: 20260420), CancellationToken.None);
        var outcome = await handler.HandleAsync(inconclusiveMessageId, BuildInconclusiveEnvelope(inconclusiveMessageId, analysisId: 20260420, reason: "CR-7 / WV-318 QA escape; no alarm, no telemetry.", alarmFloodId: null), CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, outcome);

        await using var verifyContext = CreateDbContext();
        var summary = await verifyContext.CaseSummaries.SingleAsync();
        Assert.Equal(ReportingCaseStatus.Inconclusive, summary.Status);
        Assert.Null(summary.AlarmFloodId); // genuinely flood-less, projected as NULL not a sentinel
        Assert.Contains("WV-318", summary.Reason);
        Assert.Null(summary.Verdict);
        Assert.Equal(0, await verifyContext.PoisonMessages.CountAsync());
    }

    [Fact]
    public async Task Out_of_order_inconclusive_delivery_buffers_then_applies_once_the_case_opens()
    {
        var handler = BuildHandler();
        var inconclusiveMessageId = Guid.NewGuid();
        var openedMessageId = Guid.NewGuid();

        // Inconclusive arrives BEFORE its case-opened event.
        var inconclusiveOutcome = await handler.HandleAsync(inconclusiveMessageId, BuildInconclusiveEnvelope(inconclusiveMessageId, analysisId: 801), CancellationToken.None);
        Assert.Equal(MessageHandlingOutcome.Ack, inconclusiveOutcome);

        await using (var midContext = CreateDbContext())
        {
            Assert.Equal(0, await midContext.CaseSummaries.CountAsync());
            var pending = await midContext.PendingInconclusives.SingleAsync();
            Assert.Equal(801, pending.AnalysisId);
        }

        var openedOutcome = await handler.HandleAsync(openedMessageId, BuildOpenedEnvelope(openedMessageId, analysisId: 801), CancellationToken.None);
        Assert.Equal(MessageHandlingOutcome.Ack, openedOutcome);

        await using var verifyContext = CreateDbContext();
        var summary = await verifyContext.CaseSummaries.SingleAsync();
        Assert.Equal(ReportingCaseStatus.Inconclusive, summary.Status);
        Assert.Equal("Records incomplete; provenance trail could not be established.", summary.Reason);
        Assert.Equal(0, await verifyContext.PendingInconclusives.CountAsync());
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
