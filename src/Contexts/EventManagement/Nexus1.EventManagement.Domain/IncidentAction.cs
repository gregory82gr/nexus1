using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>
/// Corrective or preventive action arising from an Incident (atlas C.8.1,
/// query 3's subject: "open incident actions that are overdue or nearing due
/// date"). IncidentId, IncidentActionTypeId, IncidentActionStatusId are real
/// internal FKs. VerifiedByUserId is passport-only (Security.ApplicationUser,
/// a different physical database).
///
/// Real lifecycle (ADR-022, matching query 3's own framing): created via
/// Create with no CompletedAtUtc/VerifiedAtUtc set, then Complete(DateTime)
/// records completion, then Verify(DateTime, int?) records verification by
/// an optional user. Both are idempotency-guarded — calling either out of
/// order or twice is a domain error, not silently accepted, since the
/// atlas's own IncidentActionStatus lookup ("proposed, assigned, in
/// progress, completed, verified, cancelled, overdue") models this as a
/// real, ordered lifecycle. The IncidentActionStatusId itself is a passport
/// value supplied by the caller/Application layer (this Domain type does not
/// hard-code which status codes mean what); Complete/Verify only guard the
/// DueAtUtc-adjacent timestamp fields.
///
/// Audit columns (CreatedBy, ModifiedAtUtc, ModifiedBy, RowVersion) are not
/// modeled in Domain, same restraint as every prior sector.
/// </summary>
public sealed class IncidentAction : Entity<IncidentActionId>, IAggregateRoot
{
    private IncidentAction(
        IncidentActionId id, IncidentId incidentId, IncidentActionTypeId incidentActionTypeId,
        IncidentActionStatusId incidentActionStatusId, string title, string? description, DateTime? dueAtUtc,
        DateTime? completedAtUtc, DateTime? verifiedAtUtc, int? verifiedByUserId)
        : base(id)
    {
        IncidentId = incidentId;
        IncidentActionTypeId = incidentActionTypeId;
        IncidentActionStatusId = incidentActionStatusId;
        Title = title;
        Description = description;
        DueAtUtc = dueAtUtc;
        CompletedAtUtc = completedAtUtc;
        VerifiedAtUtc = verifiedAtUtc;
        VerifiedByUserId = verifiedByUserId;
    }

    /// <summary>Real internal FK to Incident — typed to match Incident's own strongly-typed Id.</summary>
    public IncidentId IncidentId { get; }

    public IncidentActionTypeId IncidentActionTypeId { get; }

    public IncidentActionStatusId IncidentActionStatusId { get; }

    public string Title { get; }

    public string? Description { get; }

    public DateTime? DueAtUtc { get; }

    public DateTime? CompletedAtUtc { get; private set; }

    public DateTime? VerifiedAtUtc { get; private set; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-022).</summary>
    public int? VerifiedByUserId { get; private set; }

    public static IncidentAction Create(
        IncidentActionId id, long incidentId, IncidentActionTypeId incidentActionTypeId,
        IncidentActionStatusId incidentActionStatusId, string title, string? description = null, DateTime? dueAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("IncidentAction title must not be empty.", nameof(title));
        }

        return new IncidentAction(
            id, new IncidentId(incidentId), incidentActionTypeId, incidentActionStatusId, title, description, dueAtUtc,
            completedAtUtc: null, verifiedAtUtc: null, verifiedByUserId: null);
    }

    /// <summary>Records completion. An action already completed cannot be completed again.</summary>
    public void Complete(DateTime completedAtUtc)
    {
        if (CompletedAtUtc is not null)
        {
            throw new InvalidOperationException($"IncidentAction {IncidentId}/{Title} is already completed.");
        }

        CompletedAtUtc = completedAtUtc;
    }

    /// <summary>Records verification. Requires the action to already be completed; an action already verified cannot be verified again.</summary>
    public void Verify(DateTime verifiedAtUtc, int? verifiedByUserId)
    {
        if (CompletedAtUtc is null)
        {
            throw new InvalidOperationException($"IncidentAction {IncidentId}/{Title} must be completed before it can be verified.");
        }

        if (VerifiedAtUtc is not null)
        {
            throw new InvalidOperationException($"IncidentAction {IncidentId}/{Title} is already verified.");
        }

        VerifiedAtUtc = verifiedAtUtc;
        VerifiedByUserId = verifiedByUserId;
    }
}
