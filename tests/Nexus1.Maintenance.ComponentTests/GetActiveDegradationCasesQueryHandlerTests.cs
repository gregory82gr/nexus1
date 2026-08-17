using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

public sealed class GetActiveDegradationCasesQueryHandlerTests : MaintenanceComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Dedicated proof that GetActiveDegradationCasesQuery excludes closed (IsActive = false) degradation records.</summary>
    [Fact]
    public async Task Excludes_closed_degradation_records_and_counts_trend_points_for_active_ones()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await MaintenanceSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        var activeRecord = DegradationRecord.Create(
            new DegradationRecordId(1), new AssetId(seed.AssetId), new DegradationMechanismId(seed.DegradationMechanismId),
            new FindingSeverityId(seed.FindingSeverityId), NowUtc, "Vibration trending upward on pump bearing.");

        var closedRecord = DegradationRecord.Create(
            new DegradationRecordId(2), new AssetId(seed.AssetId), new DegradationMechanismId(seed.DegradationMechanismId),
            new FindingSeverityId(seed.FindingSeverityId), NowUtc.AddDays(-90), "Historic corrosion finding, since repaired.");
        closedRecord.Close(NowUtc.AddDays(-10));

        await using (var seedDegradationContext = CreateDbContext())
        {
            await seedDegradationContext.DegradationRecords.AddAsync(activeRecord);
            await seedDegradationContext.DegradationRecords.AddAsync(closedRecord);
            await seedDegradationContext.SaveChangesAsync();
        }

        var trendPoint1 = DegradationTrendPoint.Create(
            new DegradationTrendPointId(1), activeRecord.Id, seed.EngineeringUnitId, NowUtc.AddDays(-2), 0.20);
        var trendPoint2 = DegradationTrendPoint.Create(
            new DegradationTrendPointId(2), activeRecord.Id, seed.EngineeringUnitId, NowUtc, 0.35, sourceSignalId: seed.SignalId);

        await using (var seedTrendPointContext = CreateDbContext())
        {
            await seedTrendPointContext.DegradationTrendPoints.AddAsync(trendPoint1);
            await seedTrendPointContext.DegradationTrendPoints.AddAsync(trendPoint2);
            await seedTrendPointContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetActiveDegradationCasesQueryHandler(new EfActiveDegradationCasesFinder(dbContext));

        var result = await handler.Handle(new GetActiveDegradationCasesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value);
        Assert.Equal(MaintenanceSeedHelper.AssetCode, row.AssetCode);
        Assert.Equal("VIBRATION", row.Mechanism);
        Assert.Equal("MAJOR", row.Severity);
        Assert.Equal(2, row.TrendPoints);
    }
}
