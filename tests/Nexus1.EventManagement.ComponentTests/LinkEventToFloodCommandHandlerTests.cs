using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

public sealed class LinkEventToFloodCommandHandlerTests : EventManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static LinkEventToFloodCommandHandler CreateHandler(EventManagementDbContext dbContext) => new(
        new EfRepository<OperationalEvent, OperationalEventId>(dbContext),
        new EfRepository<EventFloodLink, EventFloodLinkId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    private static async Task<long> SeedOperationalEventAsync(EventManagementDbContext dbContext, EventManagementSeedHelper.SeedResult seed)
    {
        var operationalEvent = OperationalEvent.Create(
            new OperationalEventId(1), seed.UnitId, new EventTypeId(seed.EventTypeId), new EventStatusId(seed.EventStatusId),
            new EventSeverityId(seed.EventSeverityId), new EventSourceTypeId(seed.EventSourceTypeId),
            EventManagementSeedHelper.EventCode, "Feedwater flow deviation", NowUtc, NowUtc.AddMinutes(5));
        await dbContext.OperationalEvents.AddAsync(operationalEvent);
        await dbContext.SaveChangesAsync();
        return operationalEvent.Id.Value;
    }

    [Fact]
    public async Task Links_the_event_to_the_seeded_alarm_flood()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);
        long operationalEventId;
        await using (var eventSeedContext = CreateDbContext())
        {
            operationalEventId = await SeedOperationalEventAsync(eventSeedContext, seed);
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new LinkEventToFloodCommand(operationalEventId, seed.AlarmFloodId), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.EventFloodLinks.FindAsync(new EventFloodLinkId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(seed.AlarmFloodId, stored!.AlarmFloodId);
        Assert.Equal("TRIGGER", stored.LinkRole);
    }

    [Fact]
    public async Task Fails_when_the_referenced_operational_event_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new LinkEventToFloodCommand(OperationalEventId: 9999L, seed.AlarmFloodId), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
