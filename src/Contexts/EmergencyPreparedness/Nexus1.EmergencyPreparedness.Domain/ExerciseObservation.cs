using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// An append-only finding record produced during an Exercise (ADR-025) —
/// mirrors RadiationMonitoring.RadiationReading's/Robotics.RobotHealthSnapshot's
/// own append-only shape. ExerciseId and ObservationSeverityId are real
/// internal FKs, NOT NULL. ExerciseInjectId is deliberately omitted entirely
/// (not even as a passport column) — its target table (ExerciseInject) is
/// out of scope this pass.
///
/// ObservedByUserId is deliberately downgraded to a plain passport int, no
/// enforced FK — Security.ApplicationUser lives in SecurityDb (ADR-025).
///
/// No audit columns at all — append-only, all fields modeled directly in
/// Domain.
/// </summary>
public sealed class ExerciseObservation : Entity<ExerciseObservationId>, IAggregateRoot
{
    private ExerciseObservation(
        ExerciseObservationId id, ExerciseId exerciseId, ObservationSeverityId observationSeverityId,
        int observedByUserId, DateTime observedAtUtc, string findingText, bool correctiveActionRequired)
        : base(id)
    {
        ExerciseId = exerciseId;
        ObservationSeverityId = observationSeverityId;
        ObservedByUserId = observedByUserId;
        ObservedAtUtc = observedAtUtc;
        FindingText = findingText;
        CorrectiveActionRequired = correctiveActionRequired;
    }

    public ExerciseId ExerciseId { get; }

    public ObservationSeverityId ObservationSeverityId { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-025).</summary>
    public int ObservedByUserId { get; }

    public DateTime ObservedAtUtc { get; }

    public string FindingText { get; }

    public bool CorrectiveActionRequired { get; }

    public static ExerciseObservation Create(
        ExerciseObservationId id, ExerciseId exerciseId, ObservationSeverityId observationSeverityId,
        int observedByUserId, DateTime observedAtUtc, string findingText, bool correctiveActionRequired = false)
    {
        if (string.IsNullOrWhiteSpace(findingText))
        {
            throw new ArgumentException("ExerciseObservation finding text must not be empty.", nameof(findingText));
        }

        return new ExerciseObservation(
            id, exerciseId, observationSeverityId, observedByUserId, observedAtUtc, findingText,
            correctiveActionRequired);
    }
}
