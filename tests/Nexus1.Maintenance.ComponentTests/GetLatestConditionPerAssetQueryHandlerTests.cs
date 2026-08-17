using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

public sealed class GetLatestConditionPerAssetQueryHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_most_recently_assessed_condition_per_asset()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        var olderCondition = AssetCondition.Create(
            new AssetConditionId(1), new AssetId(seed.AssetId), new ConditionGradeId(seed.ConditionGradeId),
            NowUtc.AddDays(-30), healthScorePercent: 90m);

        var latestCondition = AssetCondition.Create(
            new AssetConditionId(2), new AssetId(seed.AssetId), new ConditionGradeId(seed.ConditionGradeId),
            NowUtc, healthScorePercent: 72.5m, remainingUsefulLifeDays: 400);

        await using (var seedConditionContext = CreateDbContext())
        {
            await seedConditionContext.AssetConditions.AddAsync(olderCondition);
            await seedConditionContext.AssetConditions.AddAsync(latestCondition);
            await seedConditionContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetLatestConditionPerAssetQueryHandler(new EfLatestConditionPerAssetFinder(dbContext));

        var result = await handler.Handle(new GetLatestConditionPerAssetQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value);
        Assert.Equal(MaintenanceSeedHelper.AssetCode, row.AssetCode);
        Assert.Equal(NowUtc, row.AssessedAtUtc);
        Assert.Equal(72.5m, row.HealthScorePercent);
        Assert.Equal(400, row.RemainingUsefulLifeDays);
        Assert.Equal("B", row.ConditionGrade);
    }
}
