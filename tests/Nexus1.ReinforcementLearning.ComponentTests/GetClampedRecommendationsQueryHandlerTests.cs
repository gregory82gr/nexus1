using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

public sealed class GetClampedRecommendationsQueryHandlerTests : ReinforcementLearningComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_clamped_recommendation()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await using var seedContext = CreateDbContext();
        await ReinforcementLearningSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, digitalTwinContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetClampedRecommendationsQueryHandler(new EfClampedRecommendationsFinder(dbContext));

        var result = await handler.Handle(new GetClampedRecommendationsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var recommendation = Assert.Single(result.Value);
        Assert.Equal("S1", recommendation.StateCode);
        Assert.Equal("A1", recommendation.RecommendedActionCode);
        Assert.Equal("A0", recommendation.ClampedActionCode);
        Assert.Equal("Clamped to validated band.", recommendation.ClampReason);
    }
}
