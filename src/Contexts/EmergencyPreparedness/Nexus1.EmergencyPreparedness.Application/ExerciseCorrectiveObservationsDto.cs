namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>Atlas verification query 2, verbatim: exercises with a count of ExerciseObservation rows where CorrectiveActionRequired = 1.</summary>
public sealed record ExerciseCorrectiveObservationsDto(string ExerciseCode, string ExerciseName, int CorrectiveObservationCount);
