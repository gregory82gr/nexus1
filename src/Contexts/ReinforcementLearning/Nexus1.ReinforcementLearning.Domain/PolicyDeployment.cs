using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// A Policy made live for a real ReactorFleet.Unit, in a given AdvisoryMode
/// (atlas C.11.2) — the start of the advisory pipeline: PolicyDeployment ->
/// AdvisorySession -> AdvisoryRecommendation. DeployedByUserId is
/// passport-only (Security.ApplicationUser lives in SecurityDb, ADR-026).
/// Only RowVersion for audit — no Created/Modified columns at all,
/// verified against the real DDL (narrower than every other substantive
/// table in this sector). Not modeled in Domain — EF shadow property only.
/// </summary>
public sealed class PolicyDeployment : Entity<PolicyDeploymentId>, IAggregateRoot
{
    private PolicyDeployment(
        PolicyDeploymentId id, PolicyId policyId, AdvisoryModeId advisoryModeId, int unitId, int? deployedByUserId,
        DateTime deployedAtUtc, DateTime? retiredAtUtc, bool isActive, string? deploymentNote)
        : base(id)
    {
        PolicyId = policyId;
        AdvisoryModeId = advisoryModeId;
        UnitId = unitId;
        DeployedByUserId = deployedByUserId;
        DeployedAtUtc = deployedAtUtc;
        RetiredAtUtc = retiredAtUtc;
        IsActive = isActive;
        DeploymentNote = deploymentNote;
    }

    public PolicyId PolicyId { get; }

    public AdvisoryModeId AdvisoryModeId { get; }

    /// <summary>ReactorFleet.Unit real FK (ADR-026).</summary>
    public int UnitId { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-026).</summary>
    public int? DeployedByUserId { get; }

    public DateTime DeployedAtUtc { get; }

    public DateTime? RetiredAtUtc { get; }

    public bool IsActive { get; }

    public string? DeploymentNote { get; }

    public static PolicyDeployment Create(
        PolicyDeploymentId id, PolicyId policyId, AdvisoryModeId advisoryModeId, int unitId, DateTime deployedAtUtc,
        int? deployedByUserId = null, DateTime? retiredAtUtc = null, bool isActive = true,
        string? deploymentNote = null)
    {
        return new PolicyDeployment(
            id, policyId, advisoryModeId, unitId, deployedByUserId, deployedAtUtc, retiredAtUtc, isActive,
            deploymentNote);
    }
}
