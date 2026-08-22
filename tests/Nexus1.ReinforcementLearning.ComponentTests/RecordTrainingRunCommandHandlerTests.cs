using Nexus1.BuildingBlocks.Application;
using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

public sealed class RecordTrainingRunCommandHandlerTests : ReinforcementLearningComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    private static RecordTrainingRunCommandHandler CreateHandler(ReinforcementLearningDbContext dbContext) => new(
        new EfRepository<TrainingRun, TrainingRunId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Records_a_new_training_run_against_the_seeded_seven_way_configuration()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await ReinforcementLearningSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, digitalTwinContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordTrainingRunCommand(
                seed.ExperimentId, seed.EnvironmentModelId, seed.StateSpaceId, seed.ActionSpaceId,
                seed.RewardFunctionId, seed.HyperparameterSetId, seed.LearningAlgorithmId, seed.TrainingRunStatusId,
                "TR-002", RunSeed: 99),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.TrainingRuns.FindAsync(new TrainingRunId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("TR-002", stored!.Code);
        Assert.Equal(0, stored.EpisodeCountCompleted);
        Assert.Equal(99, stored.RunSeed);
    }
}
