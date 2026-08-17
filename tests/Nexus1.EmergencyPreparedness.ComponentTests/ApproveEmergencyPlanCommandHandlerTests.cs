using Nexus1.BuildingBlocks.Application;
using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Domain;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

public sealed class ApproveEmergencyPlanCommandHandlerTests : EmergencyPreparednessComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static ApproveEmergencyPlanCommandHandler CreateHandler(EmergencyPreparednessDbContext dbContext) => new(
        new EfRepository<EmergencyPlan, EmergencyPlanId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Approves_a_new_plan_against_the_seeded_plan_status()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ApproveEmergencyPlanCommand("EP-002", "Turbine Building Emergency Plan", seed.PlanStatusId, EmergencyPreparednessSeedHelper.SiteId, OwnerUserId: 601),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.EmergencyPlans.FindAsync(new EmergencyPlanId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("EP-002", stored!.Code);
        Assert.Equal(EmergencyPreparednessSeedHelper.SiteId, stored.SiteId);
        Assert.Null(stored.PlantId);
    }

    [Fact]
    public async Task Approves_a_plan_with_a_plant_id_and_no_enforced_fk_beyond_the_seeded_status()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ApproveEmergencyPlanCommand(
                "EP-003", "Reactor Building Emergency Plan", seed.PlanStatusId, EmergencyPreparednessSeedHelper.SiteId,
                OwnerUserId: 601, PlantId: 77),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.EmergencyPlans.FindAsync(new EmergencyPlanId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(77, stored!.PlantId);
    }
}
