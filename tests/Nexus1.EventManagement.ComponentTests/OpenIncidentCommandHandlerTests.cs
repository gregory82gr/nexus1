using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

public sealed class OpenIncidentCommandHandlerTests : EventManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static OpenIncidentCommandHandler CreateHandler(EventManagementDbContext dbContext) => new(
        new EfRepository<OperationalEvent, OperationalEventId>(dbContext),
        new EfRepository<Incident, IncidentId>(dbContext),
        new EfIncidentExistenceFinder(dbContext),
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
    public async Task Opens_an_incident_for_the_seeded_operational_event()
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
            new OpenIncidentCommand(operationalEventId, seed.IncidentTypeId, seed.IncidentStatusId, "INC-2026-0007", NowUtc),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Incidents.FindAsync(new IncidentId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("INC-2026-0007", stored!.IncidentNumber);
        Assert.Equal(new OperationalEventId(operationalEventId), stored.OperationalEventId);
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
            new OpenIncidentCommand(9999L, seed.IncidentTypeId, seed.IncidentStatusId, "INC-2026-0008", NowUtc),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    /// <summary>Proves Incident's atlas-named invariant (ADR-022) is actually enforced: attempting to open a second incident for the same OperationalEvent fails with a clear conflict, not a raw database constraint violation.</summary>
    [Fact]
    public async Task Fails_with_a_clear_conflict_when_the_event_already_has_an_open_incident()
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

        await using (var firstContext = CreateDbContext())
        {
            var firstResult = await CreateHandler(firstContext).Handle(
                new OpenIncidentCommand(operationalEventId, seed.IncidentTypeId, seed.IncidentStatusId, "INC-2026-0009", NowUtc),
                CancellationToken.None);
            Assert.True(firstResult.IsSuccess);
        }

        await using var secondContext = CreateDbContext();
        var secondResult = await CreateHandler(secondContext).Handle(
            new OpenIncidentCommand(operationalEventId, seed.IncidentTypeId, seed.IncidentStatusId, "INC-2026-0010", NowUtc),
            CancellationToken.None);

        Assert.True(secondResult.IsFailure);
        Assert.Contains("already has an open incident", secondResult.Error);

        await using var verifyContext = CreateDbContext();
        var incidentCount = verifyContext.Incidents.Count(i => i.OperationalEventId == new OperationalEventId(operationalEventId));
        Assert.Equal(1, incidentCount);
    }
}
