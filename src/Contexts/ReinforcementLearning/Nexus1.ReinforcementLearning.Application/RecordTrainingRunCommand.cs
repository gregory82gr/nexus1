using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

/// <summary>TrainingRun's defining behavior (ADR-026): creates a new training run against its seven-way pinned configuration plus status.</summary>
public sealed record RecordTrainingRunCommand(
    int ExperimentId, int EnvironmentModelId, int StateSpaceId, int ActionSpaceId, int RewardFunctionId,
    int HyperparameterSetId, int LearningAlgorithmId, int TrainingRunStatusId, string Code,
    DateTime? StartedAtUtc = null, DateTime? CompletedAtUtc = null, int EpisodeCountCompleted = 0,
    decimal? TotalReward = null, decimal? AverageReward = null, int? RunSeed = null)
    : ICommand<int>;
