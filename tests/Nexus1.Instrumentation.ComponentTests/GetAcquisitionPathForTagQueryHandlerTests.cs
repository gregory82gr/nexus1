using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

public sealed class GetAcquisitionPathForTagQueryHandlerTests : InstrumentationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_full_acquisition_path_from_tag_to_raw_point()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetAcquisitionPathForTagQueryHandler(new EfAcquisitionPathFinder(dbContext));

        var result = await handler.Handle(
            new GetAcquisitionPathForTagQuery(InstrumentationSeedHelper.SignalTag), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var path = Assert.Single(result.Value);
        Assert.Equal(InstrumentationSeedHelper.SignalTag, path.Tag);
        Assert.Equal("NODE-1", path.NodeCode);
        Assert.Equal("OPC-UA", path.Protocol);
        Assert.Equal("ns=2;s=Reactor.Power", path.RawAddress);
    }

    [Fact]
    public async Task Returns_empty_for_unknown_tag()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetAcquisitionPathForTagQueryHandler(new EfAcquisitionPathFinder(dbContext));

        var result = await handler.Handle(new GetAcquisitionPathForTagQuery("NO-SUCH-TAG"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
