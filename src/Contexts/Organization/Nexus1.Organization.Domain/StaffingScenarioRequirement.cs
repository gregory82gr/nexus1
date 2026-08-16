using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// One required position or qualification inside a staffing scenario (atlas
/// C.3.4.9). No audit columns — the atlas DDL genuinely gives this table
/// none (verified against the atlas, ADR-017).
/// </summary>
public sealed class StaffingScenarioRequirement : Entity<StaffingScenarioRequirementId>, IAggregateRoot
{
    private StaffingScenarioRequirement(
        StaffingScenarioRequirementId id, StaffingScenarioId staffingScenarioId, PositionId positionId,
        QualificationId? requiredQualificationId, int requiredCount, string? notes)
        : base(id)
    {
        StaffingScenarioId = staffingScenarioId;
        PositionId = positionId;
        RequiredQualificationId = requiredQualificationId;
        RequiredCount = requiredCount;
        Notes = notes;
    }

    public StaffingScenarioId StaffingScenarioId { get; }

    public PositionId PositionId { get; }

    public QualificationId? RequiredQualificationId { get; }

    public int RequiredCount { get; }

    public string? Notes { get; }

    public static StaffingScenarioRequirement Create(
        StaffingScenarioRequirementId id, StaffingScenarioId staffingScenarioId, PositionId positionId,
        int requiredCount, QualificationId? requiredQualificationId = null, string? notes = null)
    {
        if (requiredCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredCount), requiredCount, "RequiredCount must be >= 0.");
        }

        return new StaffingScenarioRequirement(id, staffingScenarioId, positionId, requiredQualificationId, requiredCount, notes);
    }
}
