using Nexus1.BuildingBlocks.Application;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

public sealed class RecordAssetConditionCommandHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static RecordAssetConditionCommandHandler CreateHandler(MaintenanceDbContext dbContext) => new(
        new EfRepository<Asset, AssetId>(dbContext),
        new EfRepository<AssetCondition, AssetConditionId>(dbContext),
        new EfRepository<AssetConditionMeasurement, AssetConditionMeasurementId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Writes_a_condition_and_its_measurements_in_one_operation()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordAssetConditionCommand(
                seed.AssetId, seed.ConditionGradeId, NowUtc,
                [new AssetConditionMeasurementRequest(seed.EngineeringUnitId, 0.42, NowUtc, seed.SignalId)],
                HealthScorePercent: 88m, RemainingUsefulLifeDays: 600),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var storedCondition = await verifyContext.AssetConditions.FindAsync(new AssetConditionId(result.Value));
        Assert.NotNull(storedCondition);
        Assert.Equal(88m, storedCondition!.HealthScorePercent);

        var storedMeasurements = verifyContext.AssetConditionMeasurements
            .Where(m => m.AssetConditionId == storedCondition.Id).ToList();
        var storedMeasurement = Assert.Single(storedMeasurements);
        Assert.Equal(0.42, storedMeasurement.MeasuredValue);
        Assert.Equal(seed.SignalId, storedMeasurement.SignalId);
    }

    [Fact]
    public async Task Fails_when_the_asset_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordAssetConditionCommand(
                999, seed.ConditionGradeId, NowUtc, [new AssetConditionMeasurementRequest(seed.EngineeringUnitId, 0.1, NowUtc)]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    /// <summary>Drives AssetCondition.Create's CK_Maintenance_AssetCondition_HealthScore invariant through the real command-handler path, not just the Domain layer directly.</summary>
    [Fact]
    public async Task Fails_when_health_score_is_out_of_range()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordAssetConditionCommand(
                seed.AssetId, seed.ConditionGradeId, NowUtc, [], HealthScorePercent: 150m),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
