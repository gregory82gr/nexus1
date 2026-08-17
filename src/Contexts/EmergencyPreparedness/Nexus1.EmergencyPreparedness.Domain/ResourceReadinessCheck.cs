using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// A periodic readiness/calibration check record for an EmergencyResource
/// (ADR-025), atlas query 4's own subject — the correlated-subquery
/// "latest per parent row" pattern applies here, matching
/// RadiationMonitoring.RadiationReading's own role for
/// GetLatestReadingPerMonitorQuery. EmergencyResourceId and
/// ReadinessStatusId are real internal FKs, NOT NULL.
///
/// CheckedByUserId is deliberately downgraded to a plain passport int, no
/// enforced FK — Security.ApplicationUser lives in SecurityDb (ADR-025).
///
/// No audit columns beyond RowVersion (mapped as an EF-only shadow
/// property) — matches the real DDL exactly.
/// </summary>
public sealed class ResourceReadinessCheck : Entity<ResourceReadinessCheckId>, IAggregateRoot
{
    private ResourceReadinessCheck(
        ResourceReadinessCheckId id, EmergencyResourceId emergencyResourceId, ReadinessStatusId readinessStatusId,
        DateTime checkedAtUtc, int checkedByUserId, string conditionSummary, DateTime? nextCheckDueUtc)
        : base(id)
    {
        EmergencyResourceId = emergencyResourceId;
        ReadinessStatusId = readinessStatusId;
        CheckedAtUtc = checkedAtUtc;
        CheckedByUserId = checkedByUserId;
        ConditionSummary = conditionSummary;
        NextCheckDueUtc = nextCheckDueUtc;
    }

    public EmergencyResourceId EmergencyResourceId { get; }

    public ReadinessStatusId ReadinessStatusId { get; }

    public DateTime CheckedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-025).</summary>
    public int CheckedByUserId { get; }

    public string ConditionSummary { get; }

    public DateTime? NextCheckDueUtc { get; }

    public static ResourceReadinessCheck Create(
        ResourceReadinessCheckId id, EmergencyResourceId emergencyResourceId, ReadinessStatusId readinessStatusId,
        DateTime checkedAtUtc, int checkedByUserId, string conditionSummary, DateTime? nextCheckDueUtc = null)
    {
        if (string.IsNullOrWhiteSpace(conditionSummary))
        {
            throw new ArgumentException("ResourceReadinessCheck condition summary must not be empty.", nameof(conditionSummary));
        }

        return new ResourceReadinessCheck(
            id, emergencyResourceId, readinessStatusId, checkedAtUtc, checkedByUserId, conditionSummary,
            nextCheckDueUtc);
    }
}
