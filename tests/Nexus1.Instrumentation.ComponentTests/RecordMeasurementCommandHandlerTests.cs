using Nexus1.BuildingBlocks.Application;
using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

public sealed class RecordMeasurementCommandHandlerTests : InstrumentationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static RecordMeasurementCommandHandler CreateHandler(InstrumentationDbContext dbContext) => new(
        new EfRepository<Signal, SignalId>(dbContext), new EfRepository<Measurement, MeasurementId>(dbContext),
        UnitOfWork(dbContext), new FixedDateTimeProvider(NowUtc), new SequentialIdGenerator());

    [Fact]
    public async Task Records_a_measurement_with_a_numeric_value()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordMeasurementCommand(seed.SignalId, seed.SignalQualityGoodId, seed.MeasurementSourceId, NowUtc, NumericValue: 87.5),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Measurements.FindAsync(new MeasurementId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(87.5, stored!.NumericValue);
    }

    [Fact]
    public async Task Fails_when_the_signal_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordMeasurementCommand(999, seed.SignalQualityGoodId, seed.MeasurementSourceId, NowUtc, NumericValue: 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    /// <summary>Drives Measurement.Create's CK_Instrumentation_Measurement_OneValue invariant through the real command-handler path, not just the Domain layer directly.</summary>
    [Fact]
    public async Task Fails_when_neither_numeric_nor_text_value_is_supplied()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordMeasurementCommand(seed.SignalId, seed.SignalQualityGoodId, seed.MeasurementSourceId, NowUtc),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("NumericValue", result.Error);
    }
}
