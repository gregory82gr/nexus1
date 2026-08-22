using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

public sealed class GetActiveHistorizedSignalsForUnitQueryHandlerTests : InstrumentationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_active_historized_signals_for_the_seeded_unit()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetActiveHistorizedSignalsForUnitQueryHandler(new EfActiveHistorizedSignalFinder(dbContext));

        var result = await handler.Handle(
            new GetActiveHistorizedSignalsForUnitQuery(InstrumentationSeedHelper.UnitCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var signal = Assert.Single(result.Value);
        Assert.Equal(InstrumentationSeedHelper.SignalTag, signal.Tag);
        Assert.Equal("POWER", signal.CategoryCode);
        Assert.Equal(InstrumentationSeedHelper.EngineeringUnitSymbol, signal.UnitSymbol);
        Assert.Equal("STANDARD", signal.RetentionCode);
    }

    [Fact]
    public async Task Returns_empty_for_unit_code_with_no_signals()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetActiveHistorizedSignalsForUnitQueryHandler(new EfActiveHistorizedSignalFinder(dbContext));

        var result = await handler.Handle(new GetActiveHistorizedSignalsForUnitQuery("NX1-U2"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
