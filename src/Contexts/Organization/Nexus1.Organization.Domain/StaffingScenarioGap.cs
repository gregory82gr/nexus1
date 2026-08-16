using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// Gap row showing required, available, and missing personnel counts (atlas
/// C.3.4.9). GapCount is a real domain invariant, not a caller-supplied
/// value: the atlas models it as a SQL computed column
/// (<c>CASE WHEN RequiredCount &gt; AvailableCount THEN RequiredCount -
/// AvailableCount ELSE 0 END</c>) and this factory computes it identically,
/// so the database's computed column and the domain's own invariant agree
/// by construction (ADR-017). No audit columns — the atlas DDL genuinely
/// gives this table none.
/// </summary>
public sealed class StaffingScenarioGap : Entity<StaffingScenarioGapId>, IAggregateRoot
{
    private StaffingScenarioGap(
        StaffingScenarioGapId id, StaffingScenarioResultId staffingScenarioResultId, PositionId positionId,
        int requiredCount, int availableCount, int gapCount, string? notes)
        : base(id)
    {
        StaffingScenarioResultId = staffingScenarioResultId;
        PositionId = positionId;
        RequiredCount = requiredCount;
        AvailableCount = availableCount;
        GapCount = gapCount;
        Notes = notes;
    }

    public StaffingScenarioResultId StaffingScenarioResultId { get; }

    public PositionId PositionId { get; }

    public int RequiredCount { get; }

    public int AvailableCount { get; }

    /// <summary>Computed by this factory, never accepted as a caller-supplied value — mirrors the atlas's own SQL computed column exactly.</summary>
    public int GapCount { get; }

    public string? Notes { get; }

    public static StaffingScenarioGap Create(
        StaffingScenarioGapId id, StaffingScenarioResultId staffingScenarioResultId, PositionId positionId,
        int requiredCount, int availableCount, string? notes = null)
    {
        if (requiredCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredCount), requiredCount, "RequiredCount must be >= 0.");
        }

        if (availableCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableCount), availableCount, "AvailableCount must be >= 0.");
        }

        var gapCount = requiredCount > availableCount ? requiredCount - availableCount : 0;

        return new StaffingScenarioGap(id, staffingScenarioResultId, positionId, requiredCount, availableCount, gapCount, notes);
    }
}
