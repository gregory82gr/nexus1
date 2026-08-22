using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

public sealed class GetPolicyGridQueryHandlerTests : ReinforcementLearningComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_policy_grid_ordered_by_state_index()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await ReinforcementLearningSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, digitalTwinContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetPolicyGridQueryHandler(new EfPolicyGridFinder(dbContext));

        var result = await handler.Handle(new GetPolicyGridQuery(seed.PolicyId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(0, result.Value[0].StateIndex);
        Assert.Equal("S0", result.Value[0].StateCode);
        Assert.Equal("A0", result.Value[0].BestActionCode);
        Assert.Equal(1, result.Value[1].StateIndex);
        Assert.Equal("S1", result.Value[1].StateCode);
        Assert.Equal("A1", result.Value[1].BestActionCode);
    }
}
