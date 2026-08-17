using Microsoft.EntityFrameworkCore;
using Nexus1.EmergencyPreparedness.Application;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own verification query 2, verbatim: Exercise rows
/// with a count of their ExerciseObservation rows where
/// CorrectiveActionRequired = 1, restricted to exercises that have at least
/// one such observation. Uses a correlated COUNT subquery (via `let`)
/// rather than GroupBy+Join.
/// </summary>
internal sealed class EfExercisesWithCorrectiveObservationsFinder(EmergencyPreparednessDbContext dbContext)
    : IExercisesWithCorrectiveObservationsFinder
{
    public async Task<IReadOnlyList<ExerciseCorrectiveObservationsDto>> GetExercisesWithCorrectiveObservationsAsync(CancellationToken cancellationToken)
    {
        var query =
            from e in dbContext.Exercises
            let correctiveCount = dbContext.ExerciseObservations
                .Count(o => o.ExerciseId == e.Id && o.CorrectiveActionRequired)
            where correctiveCount > 0
            select new ExerciseCorrectiveObservationsDto(e.Code, e.Name, correctiveCount);

        return await query.ToListAsync(cancellationToken);
    }
}
