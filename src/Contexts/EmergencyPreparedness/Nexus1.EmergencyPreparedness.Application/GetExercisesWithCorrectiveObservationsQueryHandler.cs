using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

public sealed class GetExercisesWithCorrectiveObservationsQueryHandler(IExercisesWithCorrectiveObservationsFinder finder)
    : IQueryHandler<GetExercisesWithCorrectiveObservationsQuery, IReadOnlyList<ExerciseCorrectiveObservationsDto>>
{
    public async Task<Result<IReadOnlyList<ExerciseCorrectiveObservationsDto>>> Handle(GetExercisesWithCorrectiveObservationsQuery query, CancellationToken cancellationToken)
    {
        var exercises = await finder.GetExercisesWithCorrectiveObservationsAsync(cancellationToken);
        return Result<IReadOnlyList<ExerciseCorrectiveObservationsDto>>.Success(exercises);
    }
}
