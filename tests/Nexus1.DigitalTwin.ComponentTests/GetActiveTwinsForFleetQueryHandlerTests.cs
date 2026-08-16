using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

public sealed class GetActiveTwinsForFleetQueryHandlerTests : DigitalTwinComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_twin_with_its_unit_and_classification()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetActiveTwinsForFleetQueryHandler(new EfActiveTwinFinder(dbContext));

        var result = await handler.Handle(new GetActiveTwinsForFleetQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var twin = Assert.Single(result.Value);
        Assert.Equal(DigitalTwinSeedHelper.UnitCode, twin.UnitCode);
        Assert.Equal(DigitalTwinSeedHelper.TwinModelCode, twin.TwinCode);
        Assert.Equal("POINT_KINETICS", twin.ModelType);
        Assert.Equal("ACTIVE", twin.Status);
        Assert.Equal("VALIDATED", twin.Fidelity);
    }
}
