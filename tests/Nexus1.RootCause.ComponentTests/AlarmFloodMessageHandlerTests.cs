using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Contracts.AlarmManagement;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Infrastructure.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>Proves duplicate delivery, transient-failure retry, and retry-budget exhaustion — real LocalDB, no mocks.</summary>
public sealed class AlarmFloodMessageHandlerTests : RootCauseComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<RootCauseDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton<IIdGenerator, SequentialIdGenerator>();
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(NowUtc));
        services.AddScoped<IOutboxWriter, EfOutboxWriter>();
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private AlarmFloodMessageHandler BuildHandler() =>
        new(BuildScopeFactory(), NullLogger<AlarmFloodMessageHandler>.Instance);

    private static byte[] BuildEnvelope(Guid messageId, long alarmFloodId, int unitId)
    {
        var payload = new AlarmFloodDetectedV1(alarmFloodId, unitId, NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.alarm-management.alarm-flood-detected.v1", 1, NowUtc,
            "alarm-management", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    /// <summary>Not valid JSON at all — fails deterministically before any business logic runs, regardless of schema.</summary>
    private static byte[] MalformedEnvelope() => "this is not json"u8.ToArray();

    [Fact]
    public async Task First_delivery_opens_an_analysis_and_records_the_receipt()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();
        var envelopeBytes = BuildEnvelope(messageId, alarmFloodId: 500, unitId: 1);

        var outcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, outcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.RootCauseAnalyses.CountAsync());
        var analysis = await verifyContext.RootCauseAnalyses.SingleAsync();
        Assert.Equal(500, analysis.AlarmFloodId.Value);
        Assert.Equal(1, analysis.UnitId.Value);

        var receipt = await verifyContext.InboxReceipts.SingleAsync();
        Assert.Equal(AlarmFloodMessageHandler.ConsumerName, receipt.ConsumerName);
        Assert.Equal(messageId, receipt.MessageId);
    }

    [Fact]
    public async Task Duplicate_delivery_of_the_same_message_does_not_open_a_second_analysis()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();
        var envelopeBytes = BuildEnvelope(messageId, alarmFloodId: 500, unitId: 1);

        var firstOutcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);
        var secondOutcome = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);

        Assert.Equal(MessageHandlingOutcome.Ack, firstOutcome);
        Assert.Equal(MessageHandlingOutcome.Ack, secondOutcome); // a confirmed duplicate still acks — it must not be redelivered forever

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.RootCauseAnalyses.CountAsync());
        Assert.Equal(1, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task Different_messages_for_the_same_flood_are_processed_independently()
    {
        var handler = BuildHandler();

        var firstMessageId = Guid.NewGuid();
        var secondMessageId = Guid.NewGuid();
        await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, 500, 1), CancellationToken.None);
        await handler.HandleAsync(secondMessageId, BuildEnvelope(secondMessageId, 501, 1), CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(2, await verifyContext.RootCauseAnalyses.CountAsync());
        Assert.Equal(2, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task A_transient_failure_records_a_retry_ticket_and_still_acks_the_original_delivery()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        var outcome = await handler.HandleAsync(messageId, MalformedEnvelope(), CancellationToken.None);

        // Ownership moves to the RetryTicket — the original delivery is done
        // either way, so the caller acks (ADR-009).
        Assert.Equal(MessageHandlingOutcome.Ack, outcome);

        await using var verifyContext = CreateDbContext();
        var ticket = await verifyContext.RetryTickets.SingleAsync();
        Assert.Equal(AlarmFloodMessageHandler.ConsumerName, ticket.ConsumerName);
        Assert.Equal(messageId, ticket.MessageId);
        Assert.Equal(1, ticket.Attempt);
        Assert.Equal(RetryPolicies.AlarmRead.PolicyId, ticket.PolicyId);
        Assert.Null(ticket.PublishedAtUtc);
        Assert.Equal(0, await verifyContext.RootCauseAnalyses.CountAsync());
        Assert.Equal(0, await verifyContext.PoisonMessages.CountAsync());
    }

    [Fact]
    public async Task Exhausting_the_retry_budget_quarantines_the_message_and_nacks_without_requeue()
    {
        var handler = BuildHandler();
        var messageId = Guid.NewGuid();

        MessageHandlingOutcome lastOutcome = default;
        for (var i = 0; i < RetryPolicies.AlarmRead.MaxRetryAttempts + 1; i++)
        {
            lastOutcome = await handler.HandleAsync(messageId, MalformedEnvelope(), CancellationToken.None);
        }

        Assert.Equal(MessageHandlingOutcome.NackNoRequeue, lastOutcome);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(RetryPolicies.AlarmRead.MaxRetryAttempts, await verifyContext.RetryTickets.CountAsync());

        var poison = await verifyContext.PoisonMessages.SingleAsync();
        Assert.Equal(AlarmFloodMessageHandler.ConsumerName, poison.ConsumerName);
        Assert.Equal(messageId, poison.MessageId);
        Assert.Equal("attempt-budget-exhausted", poison.TerminalReason);
        Assert.Equal(RetryPolicies.AlarmRead.MaxRetryAttempts, poison.RetryAttempts);
    }
}
