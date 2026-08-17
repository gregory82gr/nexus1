using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// The readiness-testing header (ADR-025). ExerciseTypeId/ExerciseStatusId
/// are real internal FKs, NOT NULL. EmergencyScenarioId is deliberately
/// omitted entirely (not even as a passport column) — its target table
/// (EmergencyScenario) is out of scope this pass, same treatment
/// Robotics.Robot.HomeDockingStationId got (ADR-025).
///
/// SiteId/PlantId/CoordinatorUserId are deliberately downgraded to plain
/// passport ints, no enforced FK — Organization.Site/Plant live in
/// OrganizationDb and Security.ApplicationUser lives in SecurityDb, both
/// different physical databases than AlarmManagementDb (ADR-025).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only.
/// </summary>
public sealed class Exercise : Entity<ExerciseId>, IAggregateRoot
{
    private Exercise(
        ExerciseId id, string code, string name, ExerciseTypeId exerciseTypeId, ExerciseStatusId exerciseStatusId,
        int siteId, int? plantId, DateTime scheduledStartUtc, DateTime scheduledEndUtc, DateTime? actualStartUtc,
        DateTime? actualEndUtc, int coordinatorUserId, string? summary)
        : base(id)
    {
        Code = code;
        Name = name;
        ExerciseTypeId = exerciseTypeId;
        ExerciseStatusId = exerciseStatusId;
        SiteId = siteId;
        PlantId = plantId;
        ScheduledStartUtc = scheduledStartUtc;
        ScheduledEndUtc = scheduledEndUtc;
        ActualStartUtc = actualStartUtc;
        ActualEndUtc = actualEndUtc;
        CoordinatorUserId = coordinatorUserId;
        Summary = summary;
    }

    public string Code { get; }

    public string Name { get; }

    public ExerciseTypeId ExerciseTypeId { get; }

    public ExerciseStatusId ExerciseStatusId { get; }

    /// <summary>Passport-only — Organization.Site lives in OrganizationDb (ADR-025).</summary>
    public int SiteId { get; }

    /// <summary>Passport-only — Organization.Plant lives in OrganizationDb (ADR-025).</summary>
    public int? PlantId { get; }

    public DateTime ScheduledStartUtc { get; }

    public DateTime ScheduledEndUtc { get; }

    public DateTime? ActualStartUtc { get; }

    public DateTime? ActualEndUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-025).</summary>
    public int CoordinatorUserId { get; }

    public string? Summary { get; }

    public static Exercise Create(
        ExerciseId id, string code, string name, ExerciseTypeId exerciseTypeId, ExerciseStatusId exerciseStatusId,
        int siteId, DateTime scheduledStartUtc, DateTime scheduledEndUtc, int coordinatorUserId,
        int? plantId = null, DateTime? actualStartUtc = null, DateTime? actualEndUtc = null,
        string? summary = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Exercise code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Exercise name must not be empty.", nameof(name));
        }

        return new Exercise(
            id, code, name, exerciseTypeId, exerciseStatusId, siteId, plantId, scheduledStartUtc, scheduledEndUtc,
            actualStartUtc, actualEndUtc, coordinatorUserId, summary);
    }
}
