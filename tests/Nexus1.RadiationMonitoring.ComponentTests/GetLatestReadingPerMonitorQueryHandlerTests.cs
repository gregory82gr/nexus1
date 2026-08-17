using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

/// <summary>Matches the atlas's own C.13.5.2 query 3, verbatim: one row per monitor, most recent RadiationReading, with engineering-unit symbol and quality code.</summary>
public sealed class GetLatestReadingPerMonitorQueryHandlerTests : RadiationMonitoringComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_the_most_recent_reading_per_monitor()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var readingSeedContext = CreateDbContext())
        {
            var earlierReading = RadiationReading.Create(
                new RadiationReadingId(1), new RadiationMonitorId(seed.RadiationMonitorId),
                new MeasurementTypeId(seed.MeasurementTypeId), seed.EngineeringUnitId,
                new MeasurementQualityId(seed.MeasurementQualityId), NowUtc.AddHours(-1), 0.10m);
            var laterReading = RadiationReading.Create(
                new RadiationReadingId(2), new RadiationMonitorId(seed.RadiationMonitorId),
                new MeasurementTypeId(seed.MeasurementTypeId), seed.EngineeringUnitId,
                new MeasurementQualityId(seed.MeasurementQualityId), NowUtc, 0.22m);

            await readingSeedContext.RadiationReadings.AddAsync(earlierReading);
            await readingSeedContext.RadiationReadings.AddAsync(laterReading);
            await readingSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetLatestReadingPerMonitorQueryHandler(new EfLatestReadingPerMonitorFinder(dbContext));

        var result = await handler.Handle(new GetLatestReadingPerMonitorQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reading = Assert.Single(result.Value);
        Assert.Equal(RadiationMonitoringSeedHelper.RadiationMonitorCode, reading.MonitorCode);
        Assert.Equal(0.22m, reading.Value);
        Assert.Equal(NowUtc, reading.TimestampUtc);
        Assert.Equal(RadiationMonitoringSeedHelper.EngineeringUnitSymbol, reading.EngineeringUnitSymbol);
        Assert.Equal("VALID", reading.Quality);
    }
}
