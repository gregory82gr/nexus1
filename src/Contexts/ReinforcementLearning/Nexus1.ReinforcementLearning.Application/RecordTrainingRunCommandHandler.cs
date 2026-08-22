using Nexus1.BuildingBlocks.Application;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class RecordTrainingRunCommandHandler(
    IRepository<TrainingRun, TrainingRunId> trainingRunRepository, IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<RecordTrainingRunCommand, int>
{
    public async Task<Result<int>> Handle(RecordTrainingRunCommand command, CancellationToken cancellationToken)
    {
        TrainingRun trainingRun;
        try
        {
            trainingRun = TrainingRun.Create(
                new TrainingRunId(idGenerator.NextInt()), new ExperimentId(command.ExperimentId),
                new EnvironmentModelId(command.EnvironmentModelId), new StateSpaceId(command.StateSpaceId),
                new ActionSpaceId(command.ActionSpaceId), new RewardFunctionId(command.RewardFunctionId),
                new HyperparameterSetId(command.HyperparameterSetId), new LearningAlgorithmId(command.LearningAlgorithmId),
                new TrainingRunStatusId(command.TrainingRunStatusId), command.Code, command.StartedAtUtc,
                command.CompletedAtUtc, command.EpisodeCountCompleted, command.TotalReward, command.AverageReward,
                command.RunSeed);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await trainingRunRepository.AddAsync(trainingRun, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(trainingRun.Id.Value);
    }
}
