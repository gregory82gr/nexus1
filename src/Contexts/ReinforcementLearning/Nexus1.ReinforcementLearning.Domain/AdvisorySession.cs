using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// A live consultation session against a deployed policy for a real
/// ReactorFleet.Unit — Chapter 10's synchronous, in-process consultation
/// design realized as a persisted session header (atlas C.11.2, ADR-026).
/// StartedByUserId is passport-only (Security.ApplicationUser lives in
/// SecurityDb). StartedAtUtc has a SQL DEFAULT (SYSUTCDATETIME()) but is
/// still a required constructor param, matching QTable.SnapshotAtUtc's own
/// "SQL default exists but Domain still always supplies it" pattern. No
/// audit columns beyond what's listed — no RowVersion for this table,
/// verified against the real DDL.
/// </summary>
public sealed class AdvisorySession : Entity<AdvisorySessionId>, IAggregateRoot
{
    private AdvisorySession(
        AdvisorySessionId id, PolicyDeploymentId policyDeploymentId, int unitId, int? startedByUserId,
        DateTime startedAtUtc, DateTime? endedAtUtc, string? sessionNote)
        : base(id)
    {
        PolicyDeploymentId = policyDeploymentId;
        UnitId = unitId;
        StartedByUserId = startedByUserId;
        StartedAtUtc = startedAtUtc;
        EndedAtUtc = endedAtUtc;
        SessionNote = sessionNote;
    }

    public PolicyDeploymentId PolicyDeploymentId { get; }

    /// <summary>ReactorFleet.Unit real FK (ADR-026).</summary>
    public int UnitId { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-026).</summary>
    public int? StartedByUserId { get; }

    public DateTime StartedAtUtc { get; }

    public DateTime? EndedAtUtc { get; }

    public string? SessionNote { get; }

    public static AdvisorySession Create(
        AdvisorySessionId id, PolicyDeploymentId policyDeploymentId, int unitId, DateTime startedAtUtc,
        int? startedByUserId = null, DateTime? endedAtUtc = null, string? sessionNote = null)
    {
        return new AdvisorySession(id, policyDeploymentId, unitId, startedByUserId, startedAtUtc, endedAtUtc, sessionNote);
    }
}
