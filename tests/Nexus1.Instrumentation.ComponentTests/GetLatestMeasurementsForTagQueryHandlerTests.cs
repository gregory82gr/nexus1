using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

public sealed class GetLatestMeasurementsForTagQueryHandlerTests : InstrumentationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Seeds 15 measurements at one-minute spacing and asserts the query returns exactly the most recent 10, newest first.</summary>
    [Fact]
    public async Task Returns_exactly_the_most_recent_10_measurements_ordered_newest_first()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var measurementContext = CreateDbContext())
        {
            for (var i = 0; i < 15; i++)
            {
                var measurement = Measurement.Create(
                    new MeasurementId(i + 1), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityGoodId),
                    new MeasurementSourceId(seed.MeasurementSourceId), NowUtc.AddMinutes(i), NowUtc, numericValue: 100 + i);
                await measurementContext.Measurements.AddAsync(measurement);
            }

            await measurementContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetLatestMeasurementsForTagQueryHandler(new EfLatestMeasurementFinder(dbContext));

        var result = await handler.Handle(
            new GetLatestMeasurementsForTagQuery(InstrumentationSeedHelper.SignalTag), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Count);

        // Newest first: minute offset 14 down to 5 (values 114 down to 105).
        Assert.Equal(114, result.Value[0].NumericValue);
        Assert.Equal(105, result.Value[9].NumericValue);
        Assert.True(result.Value.SequenceEqual(result.Value.OrderByDescending(x => x.TimestampUtc)));
    }

    [Fact]
    public async Task Returns_quality_and_source_codes()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var measurementContext = CreateDbContext())
        {
            var measurement = Measurement.Create(
                new MeasurementId(1), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityGoodId),
                new MeasurementSourceId(seed.MeasurementSourceId), NowUtc, NowUtc, numericValue: 100);
            await measurementContext.Measurements.AddAsync(measurement);
            await measurementContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetLatestMeasurementsForTagQueryHandler(new EfLatestMeasurementFinder(dbContext));

        var result = await handler.Handle(
            new GetLatestMeasurementsForTagQuery(InstrumentationSeedHelper.SignalTag), CancellationToken.None);

        var reading = Assert.Single(result.Value);
        Assert.Equal("GOOD", reading.QualityCode);
        Assert.Equal("HISTORIAN", reading.SourceCode);
    }
}
