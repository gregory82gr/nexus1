using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Contracts.AlarmManagement;
using Nexus1.RootCause.Infrastructure.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>Proves duplicate delivery of the same MessageId does not double-process — real LocalDB, no mocks.</summary>
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
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private static byte[] BuildEnvelope(Guid messageId, long alarmFloodId, int unitId)
    {
        var payload = new AlarmFloodDetectedV1(alarmFloodId, unitId, NowUtc);
        var envelope = MessageEnvelopeFactory.Build(
            messageId, "nexus1.alarm-management.alarm-flood-detected.v1", 1, NowUtc,
            "alarm-management", Guid.NewGuid(), null, payload);
        return envelope.EnvelopeBytes;
    }

    [Fact]
    public async Task First_delivery_opens_an_analysis_and_records_the_receipt()
    {
        var handler = new AlarmFloodMessageHandler(BuildScopeFactory());
        var messageId = Guid.NewGuid();
        var envelopeBytes = BuildEnvelope(messageId, alarmFloodId: 500, unitId: 1);

        var acked = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);

        Assert.True(acked);

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
        var handler = new AlarmFloodMessageHandler(BuildScopeFactory());
        var messageId = Guid.NewGuid();
        var envelopeBytes = BuildEnvelope(messageId, alarmFloodId: 500, unitId: 1);

        var firstAcked = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);
        var secondAcked = await handler.HandleAsync(messageId, envelopeBytes, CancellationToken.None);

        Assert.True(firstAcked);
        Assert.True(secondAcked); // a confirmed duplicate still acks — it must not be redelivered forever

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.RootCauseAnalyses.CountAsync());
        Assert.Equal(1, await verifyContext.InboxReceipts.CountAsync());
    }

    [Fact]
    public async Task Different_messages_for_the_same_flood_are_processed_independently()
    {
        var handler = new AlarmFloodMessageHandler(BuildScopeFactory());

        var firstMessageId = Guid.NewGuid();
        var secondMessageId = Guid.NewGuid();
        await handler.HandleAsync(firstMessageId, BuildEnvelope(firstMessageId, 500, 1), CancellationToken.None);
        await handler.HandleAsync(secondMessageId, BuildEnvelope(secondMessageId, 501, 1), CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(2, await verifyContext.RootCauseAnalyses.CountAsync());
        Assert.Equal(2, await verifyContext.InboxReceipts.CountAsync());
    }
}
