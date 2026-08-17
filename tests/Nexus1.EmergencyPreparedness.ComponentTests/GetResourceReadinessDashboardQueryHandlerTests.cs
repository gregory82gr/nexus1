using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

public sealed class GetResourceReadinessDashboardQueryHandlerTests : EmergencyPreparednessComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_one_dashboard_row_for_the_seeded_resource_with_its_latest_readiness_status()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetResourceReadinessDashboardQueryHandler(new EfResourceReadinessDashboardFinder(dbContext));

        var result = await handler.Handle(new GetResourceReadinessDashboardQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value);
        Assert.Equal(EmergencyPreparednessSeedHelper.SiteId, row.SiteId);
        Assert.Equal("EQUIPMENT", row.ResourceType);
        Assert.Equal("READY", row.ReadinessStatus);
        Assert.Equal(1, row.ResourceCount);
    }
}
