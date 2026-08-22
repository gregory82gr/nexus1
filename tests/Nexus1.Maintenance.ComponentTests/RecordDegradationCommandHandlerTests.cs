using Nexus1.BuildingBlocks.Application;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

public sealed class RecordDegradationCommandHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static RecordDegradationCommandHandler CreateHandler(MaintenanceDbContext dbContext) => new(
        new EfRepository<Asset, AssetId>(dbContext),
        new EfRepository<DegradationRecord, DegradationRecordId>(dbContext),
        new EfRepository<DegradationTrendPoint, DegradationTrendPointId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Writes_a_degradation_record_and_its_initial_trend_points_in_one_operation()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordDegradationCommand(
                seed.AssetId, seed.DegradationMechanismId, seed.FindingSeverityId, NowUtc,
                "Vibration trending upward on pump bearing.",
                [new DegradationTrendPointRequest(seed.EngineeringUnitId, NowUtc, 0.35, seed.SignalId)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var storedRecord = await verifyContext.DegradationRecords.FindAsync(new DegradationRecordId(result.Value));
        Assert.NotNull(storedRecord);
        Assert.True(storedRecord!.IsActive);
        Assert.Null(storedRecord.ClosedAtUtc);

        var storedTrendPoints = verifyContext.DegradationTrendPoints
            .Where(tp => tp.DegradationRecordId == storedRecord.Id).ToList();
        var storedTrendPoint = Assert.Single(storedTrendPoints);
        Assert.Equal(0.35, storedTrendPoint.Value);
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
            new RecordDegradationCommand(999, seed.DegradationMechanismId, seed.FindingSeverityId, NowUtc, "Bad asset reference.", []),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Fails_when_the_description_is_blank()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var eventManagementContext = CreateEventManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, eventManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordDegradationCommand(seed.AssetId, seed.DegradationMechanismId, seed.FindingSeverityId, NowUtc, "   ", []),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
