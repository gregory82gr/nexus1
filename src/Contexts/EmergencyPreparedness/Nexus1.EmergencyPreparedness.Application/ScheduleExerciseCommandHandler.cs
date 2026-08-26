using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Application;

public sealed class ScheduleExerciseCommandHandler(
    IRepository<Exercise, ExerciseId> exerciseRepository, [FromKeyedServices("EmergencyPreparedness")] IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<ScheduleExerciseCommand, int>
{
    public async Task<Result<int>> Handle(ScheduleExerciseCommand command, CancellationToken cancellationToken)
    {
        Exercise exercise;
        try
        {
            exercise = Exercise.Create(
                new ExerciseId(idGenerator.NextInt()), command.Code, command.Name,
                new ExerciseTypeId(command.ExerciseTypeId), new ExerciseStatusId(command.ExerciseStatusId),
                command.SiteId, command.ScheduledStartUtc, command.ScheduledEndUtc, command.CoordinatorUserId,
                command.PlantId, command.ActualStartUtc, command.ActualEndUtc, command.Summary);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await exerciseRepository.AddAsync(exercise, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(exercise.Id.Value);
    }
}
