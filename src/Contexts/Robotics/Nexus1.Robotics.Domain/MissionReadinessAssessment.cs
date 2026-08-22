using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>
/// The dispatch gate the sector purpose names explicitly (ADR-023): a
/// MissionReadinessAssessment records evidence about whether a mission may
/// act, it does not itself act (From_Domain_to_Twin's own "is the robot
/// output evidence or action" framing). MissionId and ReadinessStatusId are
/// real internal FKs and NOT NULL. AssessedByUserId is passport-only —
/// Security.ApplicationUser, a different physical database. No audit
/// columns.
/// </summary>
public sealed class MissionReadinessAssessment : Entity<MissionReadinessAssessmentId>, IAggregateRoot
{
    private MissionReadinessAssessment(
        MissionReadinessAssessmentId id, MissionId missionId, ReadinessStatusId readinessStatusId,
        DateTime assessedAtUtc, int? assessedByUserId, string? summary)
        : base(id)
    {
        MissionId = missionId;
        ReadinessStatusId = readinessStatusId;
        AssessedAtUtc = assessedAtUtc;
        AssessedByUserId = assessedByUserId;
        Summary = summary;
    }

    /// <summary>Real internal FK to Mission — typed to match Mission's own strongly-typed Id.</summary>
    public MissionId MissionId { get; }

    public ReadinessStatusId ReadinessStatusId { get; }

    public DateTime AssessedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-023).</summary>
    public int? AssessedByUserId { get; }

    public string? Summary { get; }

    public static MissionReadinessAssessment Create(
        MissionReadinessAssessmentId id, long missionId, ReadinessStatusId readinessStatusId, DateTime assessedAtUtc,
        int? assessedByUserId = null, string? summary = null)
    {
        return new MissionReadinessAssessment(id, new MissionId(missionId), readinessStatusId, assessedAtUtc, assessedByUserId, summary);
    }
}
