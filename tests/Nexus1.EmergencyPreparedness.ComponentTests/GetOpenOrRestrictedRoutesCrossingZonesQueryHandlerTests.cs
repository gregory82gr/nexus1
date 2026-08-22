using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

public sealed class GetOpenOrRestrictedRoutesCrossingZonesQueryHandlerTests : EmergencyPreparednessComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_open_route_and_the_radiation_zone_it_crosses()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetOpenOrRestrictedRoutesCrossingZonesQueryHandler(new EfOpenOrRestrictedRoutesFinder(dbContext));

        var result = await handler.Handle(new GetOpenOrRestrictedRoutesCrossingZonesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var route = Assert.Single(result.Value);
        Assert.Equal(EmergencyPreparednessSeedHelper.EvacuationRouteCode, route.RouteCode);
        Assert.Equal("OPEN", route.RouteStatus);
        Assert.Equal(EmergencyPreparednessSeedHelper.RadiationZoneCode, route.RadiationZoneCode);
        Assert.True(route.IsAvoidIfAlarmed);
    }
}
