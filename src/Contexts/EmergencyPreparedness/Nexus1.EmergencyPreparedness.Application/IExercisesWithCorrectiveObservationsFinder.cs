namespace Nexus1.EmergencyPreparedness.Application;

public interface IExercisesWithCorrectiveObservationsFinder
{
    Task<IReadOnlyList<ExerciseCorrectiveObservationsDto>> GetExercisesWithCorrectiveObservationsAsync(CancellationToken cancellationToken);
}
