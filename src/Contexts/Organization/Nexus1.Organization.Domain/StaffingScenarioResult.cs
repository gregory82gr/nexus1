using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// A saved evaluation of a staffing scenario (atlas C.3.4.9). No audit
/// columns — the atlas DDL genuinely gives this table none (verified
/// against the atlas, ADR-017). EvaluatedByUserId is a plain passport int,
/// not a real FK, for the same cross-database reason as StaffingScenario's
/// CreatedByUserId.
/// </summary>
public sealed class StaffingScenarioResult : Entity<StaffingScenarioResultId>, IAggregateRoot
{
    public static readonly IReadOnlyCollection<string> ValidOverallStatuses = ["Pass", "Warning", "Fail", "NotEvaluated"];

    private StaffingScenarioResult(
        StaffingScenarioResultId id, StaffingScenarioId staffingScenarioId, DateTime evaluatedAtUtc,
        int? evaluatedByUserId, string overallStatus, string? summary)
        : base(id)
    {
        StaffingScenarioId = staffingScenarioId;
        EvaluatedAtUtc = evaluatedAtUtc;
        EvaluatedByUserId = evaluatedByUserId;
        OverallStatus = overallStatus;
        Summary = summary;
    }

    public StaffingScenarioId StaffingScenarioId { get; }

    public DateTime EvaluatedAtUtc { get; }

    /// <summary>Security.ApplicationUser passport id — no enforced FK (ADR-017).</summary>
    public int? EvaluatedByUserId { get; }

    public string OverallStatus { get; }

    public string? Summary { get; }

    public static StaffingScenarioResult Create(
        StaffingScenarioResultId id, StaffingScenarioId staffingScenarioId, DateTime evaluatedAtUtc,
        string overallStatus, int? evaluatedByUserId = null, string? summary = null)
    {
        if (!ValidOverallStatuses.Contains(overallStatus))
        {
            throw new ArgumentException(
                $"OverallStatus must be one of: {string.Join(", ", ValidOverallStatuses)}.", nameof(overallStatus));
        }

        return new StaffingScenarioResult(id, staffingScenarioId, evaluatedAtUtc, evaluatedByUserId, overallStatus, summary);
    }
}
