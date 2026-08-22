using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>
/// Specialized incident record anchored to one OperationalEvent (atlas
/// C.8.1's own design choice, quoted verbatim in ADR-022): "Incidents and
/// near misses are not separate universes. Both are specialized records
/// anchored to one OperationalEvent." OperationalEventId is a real internal
/// FK to OperationalEvent.
///
/// INVARIANT (atlas-named, ADR-022): OperationalEventId is UNIQUE — at most
/// one Incident per OperationalEvent. This is a cross-aggregate uniqueness
/// rule the Domain layer cannot enforce on its own (it would require a
/// database round-trip to see other Incident rows), so it is modeled at the
/// EF configuration level as a unique index (IncidentConfiguration.cs) and
/// documented here so the invariant is visible even to a reader who never
/// opens the Infrastructure project. The Application layer's
/// OpenIncidentCommandHandler also checks for an existing Incident before
/// insert, to surface a clear conflict rather than letting the database
/// constraint violation be the only signal.
///
/// IncidentTypeId, IncidentStatusId are real internal FKs.
/// LeadInvestigatorUserId is passport-only (Security.ApplicationUser, a
/// different physical database). OpenedAtUtc defaults to "now" at the
/// Application layer, matching the atlas's own DEFAULT SYSUTCDATETIME().
///
/// Audit columns (CreatedBy, ModifiedAtUtc, ModifiedBy, RowVersion) are not
/// modeled in Domain, same restraint as every prior sector.
/// </summary>
public sealed class Incident : Entity<IncidentId>, IAggregateRoot
{
    private Incident(
        IncidentId id, OperationalEventId operationalEventId, IncidentTypeId incidentTypeId, IncidentStatusId incidentStatusId,
        string incidentNumber, string? investigationSummary, DateTime openedAtUtc, DateTime? closedAtUtc,
        int? leadInvestigatorUserId)
        : base(id)
    {
        OperationalEventId = operationalEventId;
        IncidentTypeId = incidentTypeId;
        IncidentStatusId = incidentStatusId;
        IncidentNumber = incidentNumber;
        InvestigationSummary = investigationSummary;
        OpenedAtUtc = openedAtUtc;
        ClosedAtUtc = closedAtUtc;
        LeadInvestigatorUserId = leadInvestigatorUserId;
    }

    /// <summary>Real internal FK to OperationalEvent — UNIQUE, one incident per event (see class doc). Typed to match OperationalEvent's own strongly-typed Id.</summary>
    public OperationalEventId OperationalEventId { get; }

    public IncidentTypeId IncidentTypeId { get; }

    public IncidentStatusId IncidentStatusId { get; }

    public string IncidentNumber { get; }

    public string? InvestigationSummary { get; }

    public DateTime OpenedAtUtc { get; }

    public DateTime? ClosedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-022).</summary>
    public int? LeadInvestigatorUserId { get; }

    public static Incident Create(
        IncidentId id, long operationalEventId, IncidentTypeId incidentTypeId, IncidentStatusId incidentStatusId,
        string incidentNumber, DateTime openedAtUtc, string? investigationSummary = null, DateTime? closedAtUtc = null,
        int? leadInvestigatorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(incidentNumber))
        {
            throw new ArgumentException("Incident number must not be empty.", nameof(incidentNumber));
        }

        return new Incident(
            id, new OperationalEventId(operationalEventId), incidentTypeId, incidentStatusId, incidentNumber, investigationSummary,
            openedAtUtc, closedAtUtc, leadInvestigatorUserId);
    }
}
