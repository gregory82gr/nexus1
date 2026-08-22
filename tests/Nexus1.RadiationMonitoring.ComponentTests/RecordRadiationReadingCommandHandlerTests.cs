using Nexus1.BuildingBlocks.Application;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

public sealed class RecordRadiationReadingCommandHandlerTests : RadiationMonitoringComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static RecordRadiationReadingCommandHandler CreateHandler(RadiationMonitoringDbContext dbContext) => new(
        new EfRepository<RadiationReading, RadiationReadingId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Records_a_new_reading_against_the_seeded_monitor()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordRadiationReadingCommand(
                seed.RadiationMonitorId, seed.MeasurementTypeId, seed.EngineeringUnitId, seed.MeasurementQualityId,
                NowUtc, 0.12m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.RadiationReadings.FindAsync(new RadiationReadingId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(0.12m, stored!.Value);
        Assert.False(stored.IsAlarmRelevant);
    }

    [Fact]
    public async Task Records_an_alarm_relevant_reading_with_a_source_timestamp()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordRadiationReadingCommand(
                seed.RadiationMonitorId, seed.MeasurementTypeId, seed.EngineeringUnitId, seed.MeasurementQualityId,
                NowUtc, 5.5m, IsAlarmRelevant: true, SourceTimestampUtc: NowUtc.AddSeconds(-3)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.RadiationReadings.FindAsync(new RadiationReadingId(result.Value));
        Assert.NotNull(stored);
        Assert.True(stored!.IsAlarmRelevant);
        Assert.Equal(NowUtc.AddSeconds(-3), stored.SourceTimestampUtc);
    }
}
