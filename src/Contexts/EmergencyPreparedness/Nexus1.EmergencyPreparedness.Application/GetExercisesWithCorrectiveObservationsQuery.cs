using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>Atlas verification query 2, verbatim: Exercise JOIN ExerciseObservation WHERE CorrectiveActionRequired = 1, GROUP BY.</summary>
public sealed record GetExercisesWithCorrectiveObservationsQuery : IQuery<IReadOnlyList<ExerciseCorrectiveObservationsDto>>;
