using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// One deterministic, reproducible training configuration, pinning seven
/// internal FKs plus its own status — "given the same configuration and
/// seed, you must be able to regenerate the exact 175 numbers" (ADR-026,
/// From_Trial_to_Policy Ch.11). Audit shape is CreatedAtUtc/CreatedBy/
/// ModifiedAtUtc/ModifiedBy/RowVersion — NO IsDeleted (verified against the
/// real DDL, narrower than EnvironmentModel's full six-column shape). Not
/// modeled in Domain — EF shadow properties only.
/// </summary>
public sealed class TrainingRun : Entity<TrainingRunId>, IAggregateRoot
{
    private TrainingRun(
        TrainingRunId id, ExperimentId experimentId, EnvironmentModelId environmentModelId, StateSpaceId stateSpaceId,
        ActionSpaceId actionSpaceId, RewardFunctionId rewardFunctionId, HyperparameterSetId hyperparameterSetId,
        LearningAlgorithmId learningAlgorithmId, TrainingRunStatusId trainingRunStatusId, string code,
        DateTime? startedAtUtc, DateTime? completedAtUtc, int episodeCountCompleted, decimal? totalReward,
        decimal? averageReward, int? runSeed)
        : base(id)
    {
        ExperimentId = experimentId;
        EnvironmentModelId = environmentModelId;
        StateSpaceId = stateSpaceId;
        ActionSpaceId = actionSpaceId;
        RewardFunctionId = rewardFunctionId;
        HyperparameterSetId = hyperparameterSetId;
        LearningAlgorithmId = learningAlgorithmId;
        TrainingRunStatusId = trainingRunStatusId;
        Code = code;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        EpisodeCountCompleted = episodeCountCompleted;
        TotalReward = totalReward;
        AverageReward = averageReward;
        RunSeed = runSeed;
    }

    public ExperimentId ExperimentId { get; }

    public EnvironmentModelId EnvironmentModelId { get; }

    public StateSpaceId StateSpaceId { get; }

    public ActionSpaceId ActionSpaceId { get; }

    public RewardFunctionId RewardFunctionId { get; }

    public HyperparameterSetId HyperparameterSetId { get; }

    public LearningAlgorithmId LearningAlgorithmId { get; }

    public TrainingRunStatusId TrainingRunStatusId { get; }

    public string Code { get; }

    public DateTime? StartedAtUtc { get; }

    public DateTime? CompletedAtUtc { get; }

    public int EpisodeCountCompleted { get; }

    public decimal? TotalReward { get; }

    public decimal? AverageReward { get; }

    public int? RunSeed { get; }

    public static TrainingRun Create(
        TrainingRunId id, ExperimentId experimentId, EnvironmentModelId environmentModelId, StateSpaceId stateSpaceId,
        ActionSpaceId actionSpaceId, RewardFunctionId rewardFunctionId, HyperparameterSetId hyperparameterSetId,
        LearningAlgorithmId learningAlgorithmId, TrainingRunStatusId trainingRunStatusId, string code,
        DateTime? startedAtUtc = null, DateTime? completedAtUtc = null, int episodeCountCompleted = 0,
        decimal? totalReward = null, decimal? averageReward = null, int? runSeed = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("TrainingRun code must not be empty.", nameof(code));
        }

        return new TrainingRun(
            id, experimentId, environmentModelId, stateSpaceId, actionSpaceId, rewardFunctionId, hyperparameterSetId,
            learningAlgorithmId, trainingRunStatusId, code, startedAtUtc, completedAtUtc, episodeCountCompleted,
            totalReward, averageReward, runSeed);
    }
}
