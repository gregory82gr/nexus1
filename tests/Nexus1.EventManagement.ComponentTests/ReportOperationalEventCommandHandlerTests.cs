using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

public sealed class ReportOperationalEventCommandHandlerTests : EventManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static ReportOperationalEventCommandHandler CreateHandler(EventManagementDbContext dbContext) => new(
        new EfRepository<OperationalEvent, OperationalEventId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Reports_a_new_operational_event_against_the_seeded_unit()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ReportOperationalEventCommand(
                seed.UnitId, seed.EventTypeId, seed.EventStatusId, seed.EventSeverityId, seed.EventSourceTypeId,
                EventManagementSeedHelper.EventCode, "Feedwater flow deviation", NowUtc, NowUtc.AddMinutes(5)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.OperationalEvents.FindAsync(new OperationalEventId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(EventManagementSeedHelper.EventCode, stored!.EventCode);
        Assert.False(stored.IsDrill);
    }

    [Fact]
    public async Task Reports_a_drill_with_passport_only_owner_ids()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ReportOperationalEventCommand(
                seed.UnitId, seed.EventTypeId, seed.EventStatusId, seed.EventSeverityId, seed.EventSourceTypeId,
                "EVT-2026-0500", "Quarterly evacuation drill", NowUtc, NowUtc.AddMinutes(5),
                OwnerUserId: 11, OwningPersonId: 12, IsDrill: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.OperationalEvents.FindAsync(new OperationalEventId(result.Value));
        Assert.NotNull(stored);
        Assert.True(stored!.IsDrill);
        Assert.Equal(11, stored.OwnerUserId);
        Assert.Equal(12, stored.OwningPersonId);
    }
}
