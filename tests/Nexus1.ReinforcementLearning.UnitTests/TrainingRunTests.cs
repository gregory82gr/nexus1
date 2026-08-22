using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class TrainingRunTests
{
    [Fact]
    public void Create_with_valid_seven_way_configuration_succeeds_and_leaves_optional_fields_at_defaults()
    {
        var trainingRun = TrainingRun.Create(
            new TrainingRunId(1), new ExperimentId(1), new EnvironmentModelId(1), new StateSpaceId(1),
            new ActionSpaceId(1), new RewardFunctionId(1), new HyperparameterSetId(1), new LearningAlgorithmId(1),
            new TrainingRunStatusId(1), "TR-001");

        Assert.Equal("TR-001", trainingRun.Code);
        Assert.Equal(0, trainingRun.EpisodeCountCompleted);
        Assert.Null(trainingRun.StartedAtUtc);
        Assert.Null(trainingRun.CompletedAtUtc);
        Assert.Null(trainingRun.TotalReward);
        Assert.Null(trainingRun.AverageReward);
        Assert.Null(trainingRun.RunSeed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => TrainingRun.Create(
            new TrainingRunId(1), new ExperimentId(1), new EnvironmentModelId(1), new StateSpaceId(1),
            new ActionSpaceId(1), new RewardFunctionId(1), new HyperparameterSetId(1), new LearningAlgorithmId(1),
            new TrainingRunStatusId(1), code));
    }

    [Fact]
    public void Create_with_run_seed_and_reward_totals_sets_them()
    {
        var trainingRun = TrainingRun.Create(
            new TrainingRunId(1), new ExperimentId(1), new EnvironmentModelId(1), new StateSpaceId(1),
            new ActionSpaceId(1), new RewardFunctionId(1), new HyperparameterSetId(1), new LearningAlgorithmId(1),
            new TrainingRunStatusId(1), "TR-001", episodeCountCompleted: 500, totalReward: 1234.5m,
            averageReward: 2.469m, runSeed: 42);

        Assert.Equal(500, trainingRun.EpisodeCountCompleted);
        Assert.Equal(1234.5m, trainingRun.TotalReward);
        Assert.Equal(2.469m, trainingRun.AverageReward);
        Assert.Equal(42, trainingRun.RunSeed);
    }
}
