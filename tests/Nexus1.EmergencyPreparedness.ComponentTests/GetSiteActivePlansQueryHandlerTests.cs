using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

public sealed class GetSiteActivePlansQueryHandlerTests : EmergencyPreparednessComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_active_plan_for_its_site_with_a_revision_count_of_one()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetSiteActivePlansQueryHandler(new EfSiteActivePlansFinder(dbContext));

        var result = await handler.Handle(new GetSiteActivePlansQuery(EmergencyPreparednessSeedHelper.SiteId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var plan = Assert.Single(result.Value);
        Assert.Equal(EmergencyPreparednessSeedHelper.EmergencyPlanCode, plan.PlanCode);
        Assert.Equal("APPROVED", plan.PlanStatus);
        Assert.Equal(1, plan.CurrentRevisionNumber);
        Assert.Equal(1, plan.RevisionRowCount);
    }

    [Fact]
    public async Task Returns_no_plans_for_an_unrelated_site()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetSiteActivePlansQueryHandler(new EfSiteActivePlansFinder(dbContext));

        var result = await handler.Handle(new GetSiteActivePlansQuery(999), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
