using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

public sealed class TraceModelVariableToSignalQueryHandlerTests : DigitalTwinComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_binding_from_model_variable_to_real_signal_tag()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new TraceModelVariableToSignalQueryHandler(new EfModelVariableSignalTraceFinder(dbContext));

        var result = await handler.Handle(
            new TraceModelVariableToSignalQuery(DigitalTwinSeedHelper.TwinModelCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var trace = Assert.Single(result.Value);
        Assert.Equal(DigitalTwinSeedHelper.TwinModelCode, trace.TwinCode);
        Assert.Equal(DigitalTwinSeedHelper.TwinVariableCode, trace.ModelVariable);
        Assert.Equal(DigitalTwinSeedHelper.SignalTag, trace.SignalTag);
        Assert.Equal("INPUT", trace.BindingRole);
        Assert.Equal("ACTIVE", trace.BindingStatus);
    }

    [Fact]
    public async Task Returns_empty_for_unknown_twin_code()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new TraceModelVariableToSignalQueryHandler(new EfModelVariableSignalTraceFinder(dbContext));

        var result = await handler.Handle(new TraceModelVariableToSignalQuery("NO-SUCH-TWIN"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
