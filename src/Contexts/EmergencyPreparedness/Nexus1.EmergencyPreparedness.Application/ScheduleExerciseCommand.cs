using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>Exercise's defining behavior (ADR-025): creates a new exercise against its ExerciseTypeId/ExerciseStatusId chain.</summary>
public sealed record ScheduleExerciseCommand(
    string Code, string Name, int ExerciseTypeId, int ExerciseStatusId, int SiteId, DateTime ScheduledStartUtc,
    DateTime ScheduledEndUtc, int CoordinatorUserId, int? PlantId = null, DateTime? ActualStartUtc = null,
    DateTime? ActualEndUtc = null, string? Summary = null)
    : ICommand<int>;
