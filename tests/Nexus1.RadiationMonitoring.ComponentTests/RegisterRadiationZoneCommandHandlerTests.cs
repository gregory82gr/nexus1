using Nexus1.BuildingBlocks.Application;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

public sealed class RegisterRadiationZoneCommandHandlerTests : RadiationMonitoringComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static RegisterRadiationZoneCommandHandler CreateHandler(RadiationMonitoringDbContext dbContext) => new(
        new EfRepository<RadiationZone, RadiationZoneId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Registers_a_new_zone_against_the_seeded_lookup_chain()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RegisterRadiationZoneCommand(
                seed.RadiationZoneTypeId, seed.RadiationZoneStatusActiveId, seed.RadiationAreaClassificationId,
                "RZ-002", "Turbine Building Zone 1"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.RadiationZones.FindAsync(new RadiationZoneId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("RZ-002", stored!.Code);
        Assert.Null(stored.UnitId);
    }

    [Fact]
    public async Task Registers_a_zone_with_a_unit_and_no_enforced_fk_beyond_the_seeded_unit()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RegisterRadiationZoneCommand(
                seed.RadiationZoneTypeId, seed.RadiationZoneStatusActiveId, seed.RadiationAreaClassificationId,
                "RZ-003", "Turbine Building Zone 2", UnitId: seed.UnitId, EquipmentLocationId: 99),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.RadiationZones.FindAsync(new RadiationZoneId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(seed.UnitId, stored!.UnitId);
        Assert.Equal(99, stored.EquipmentLocationId);
    }
}
