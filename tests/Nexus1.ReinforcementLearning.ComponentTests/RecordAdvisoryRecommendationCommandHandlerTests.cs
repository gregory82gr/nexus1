using Nexus1.BuildingBlocks.Application;
using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

public sealed class RecordAdvisoryRecommendationCommandHandlerTests : ReinforcementLearningComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    private static RecordAdvisoryRecommendationCommandHandler CreateHandler(ReinforcementLearningDbContext dbContext) => new(
        new EfRepository<AdvisoryRecommendation, AdvisoryRecommendationId>(dbContext), UnitOfWork(dbContext),
        new SequentialIdGenerator());

    [Fact]
    public async Task Records_a_new_clamped_advisory_recommendation_against_the_seeded_session()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await ReinforcementLearningSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, digitalTwinContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordAdvisoryRecommendationCommand(
                seed.AdvisorySessionId, seed.RecommendationStatusId, seed.StateDefinitionId1, seed.ActionDefinitionId2,
                NowUtc, ClampedActionDefinitionId: seed.ActionDefinitionId1, WasClamped: true,
                ClampReason: "Clamped to validated band.", ConfidenceScore: 0.75m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.AdvisoryRecommendations.FindAsync(new AdvisoryRecommendationId(result.Value));
        Assert.NotNull(stored);
        Assert.True(stored!.WasClamped);
        Assert.Equal(new ActionDefinitionId(seed.ActionDefinitionId2), stored.RecommendedActionDefinitionId);
        Assert.Equal(new ActionDefinitionId(seed.ActionDefinitionId1), stored.ClampedActionDefinitionId);
    }
}
