using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

public sealed class RecordIncidentActionCommandHandlerTests : EventManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static RecordIncidentActionCommandHandler CreateHandler(EventManagementDbContext dbContext) => new(
        new EfRepository<Incident, IncidentId>(dbContext),
        new EfRepository<IncidentAction, IncidentActionId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    private static async Task<long> SeedIncidentAsync(EventManagementDbContext dbContext, EventManagementSeedHelper.SeedResult seed)
    {
        var operationalEvent = OperationalEvent.Create(
            new OperationalEventId(1), seed.UnitId, new EventTypeId(seed.EventTypeId), new EventStatusId(seed.EventStatusId),
            new EventSeverityId(seed.EventSeverityId), new EventSourceTypeId(seed.EventSourceTypeId),
            EventManagementSeedHelper.EventCode, "Feedwater flow deviation", NowUtc, NowUtc.AddMinutes(5));
        await dbContext.OperationalEvents.AddAsync(operationalEvent);
        await dbContext.SaveChangesAsync();

        var incident = Incident.Create(
            new IncidentId(1), operationalEvent.Id.Value, new IncidentTypeId(seed.IncidentTypeId),
            new IncidentStatusId(seed.IncidentStatusId), "INC-2026-0007", NowUtc);
        await dbContext.Incidents.AddAsync(incident);
        await dbContext.SaveChangesAsync();

        return incident.Id.Value;
    }

    [Fact]
    public async Task Records_an_incident_action_against_the_seeded_incident()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);
        long incidentId;
        await using (var incidentSeedContext = CreateDbContext())
        {
            incidentId = await SeedIncidentAsync(incidentSeedContext, seed);
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordIncidentActionCommand(
                incidentId, seed.IncidentActionTypeId, seed.IncidentActionStatusOpenId,
                Title: "Replace corroded valve", DueAtUtc: NowUtc.AddDays(7)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.IncidentActions.FindAsync(new IncidentActionId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("Replace corroded valve", stored!.Title);
        Assert.Equal(new IncidentId(incidentId), stored.IncidentId);
        Assert.Null(stored.CompletedAtUtc);
    }

    [Fact]
    public async Task Fails_when_the_referenced_incident_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordIncidentActionCommand(9999L, seed.IncidentActionTypeId, seed.IncidentActionStatusOpenId, "Bad incident reference"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
