using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

/// <summary>Matches the atlas's own C.13.5.2 query 1, verbatim: active radiation zones with their unit and classification.</summary>
public sealed class GetActiveRadiationZonesQueryHandlerTests : RadiationMonitoringComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_zone_with_its_unit_code_and_classification()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetActiveRadiationZonesQueryHandler(new EfActiveRadiationZonesFinder(dbContext));

        var result = await handler.Handle(new GetActiveRadiationZonesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var zone = Assert.Single(result.Value);
        Assert.Equal(RadiationMonitoringSeedHelper.RadiationZoneCode, zone.Code);
        Assert.Equal(RadiationMonitoringSeedHelper.UnitCode, zone.UnitCode);
        Assert.Equal("HIGH", zone.Classification);
        Assert.Equal("ACTIVE", zone.Status);
    }
}
