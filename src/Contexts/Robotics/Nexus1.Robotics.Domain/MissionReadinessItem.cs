using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>
/// A single named check within a MissionReadinessAssessment (ADR-023, atlas
/// query 4's own subject: readiness failures that block dispatch).
/// MissionReadinessAssessmentId and ReadinessStatusId are both real internal
/// FKs and NOT NULL. No audit columns.
///
/// MissionChecklistItemId is deliberately omitted entirely — the readiness
/// authoring group (MissionChecklist/MissionChecklistItem) is out of scope
/// for this pass and does not exist in this codebase (ADR-023).
/// </summary>
public sealed class MissionReadinessItem : Entity<MissionReadinessItemId>, IAggregateRoot
{
    private MissionReadinessItem(
        MissionReadinessItemId id, MissionReadinessAssessmentId missionReadinessAssessmentId,
        ReadinessStatusId readinessStatusId, string checkName, string? detail, bool isBlocking)
        : base(id)
    {
        MissionReadinessAssessmentId = missionReadinessAssessmentId;
        ReadinessStatusId = readinessStatusId;
        CheckName = checkName;
        Detail = detail;
        IsBlocking = isBlocking;
    }

    /// <summary>Real internal FK to MissionReadinessAssessment — typed to match its own strongly-typed Id.</summary>
    public MissionReadinessAssessmentId MissionReadinessAssessmentId { get; }

    public ReadinessStatusId ReadinessStatusId { get; }

    public string CheckName { get; }

    public string? Detail { get; }

    public bool IsBlocking { get; }

    public static MissionReadinessItem Create(
        MissionReadinessItemId id, long missionReadinessAssessmentId, ReadinessStatusId readinessStatusId,
        string checkName, string? detail = null, bool isBlocking = true)
    {
        if (string.IsNullOrWhiteSpace(checkName))
        {
            throw new ArgumentException("MissionReadinessItem check name must not be empty.", nameof(checkName));
        }

        return new MissionReadinessItem(
            id, new MissionReadinessAssessmentId(missionReadinessAssessmentId), readinessStatusId, checkName, detail, isBlocking);
    }
}
