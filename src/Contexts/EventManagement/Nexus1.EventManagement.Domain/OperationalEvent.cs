using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>
/// The central event record (atlas C.8.1's own anchor): "the one place where
/// an operational occurrence receives a title, owner, severity, status and
/// lifecycle." Base record for incidents, near misses, transients and drills
/// (ADR-022) — Incident specializes it via a unique OperationalEventId FK.
///
/// UnitId is a real SQL FOREIGN KEY to ReactorFleet.Unit via the
/// ReactorFleetUnitReference shadow-entity technique (ADR-022).
///
/// PlantId is passport-only, nullable — Organization.Plant lives in
/// OrganizationDb, a different physical database than this sector's chosen
/// home (AlarmManagementDb); no enforced FK.
///
/// EventTypeId, EventStatusId, EventSeverityId, EventSourceTypeId are all
/// real internal FKs and all NOT NULL per the real DDL — EventSourceTypeId
/// is easy to miss from the abbreviated table-list summary, caught by
/// reading the real DDL directly (ADR-022, same discipline as Maintenance's
/// AssetCriticality/WorkOrderType catch).
///
/// ReportedByUserId/OwnerUserId (Security.ApplicationUser) and
/// OwningPersonId (Organization.Person) are passport-only nullable ints —
/// both live in different physical databases than this sector's home.
///
/// IsDrill defaults to false, matching the atlas's own DEFAULT 0.
///
/// Audit columns (CreatedBy, ModifiedAtUtc, ModifiedBy, RowVersion, IsDeleted)
/// are not modeled in Domain, same restraint as every prior sector, except
/// CreatedAtUtc which mirrors Asset's own treatment... actually CreatedAtUtc
/// here is pure row-insertion bookkeeping (ReportedAtUtc is the real business
/// timestamp) and is likewise left unmapped in Domain.
/// </summary>
public sealed class OperationalEvent : Entity<OperationalEventId>, IAggregateRoot
{
    private OperationalEvent(
        OperationalEventId id, int unitId, int? plantId, EventTypeId eventTypeId, EventStatusId eventStatusId,
        EventSeverityId eventSeverityId, EventSourceTypeId eventSourceTypeId, string eventCode, string title,
        string? summary, DateTime detectedAtUtc, DateTime reportedAtUtc, DateTime? closedAtUtc,
        int? reportedByUserId, int? ownerUserId, int? owningPersonId, string? sourceReference, bool isDrill)
        : base(id)
    {
        UnitId = unitId;
        PlantId = plantId;
        EventTypeId = eventTypeId;
        EventStatusId = eventStatusId;
        EventSeverityId = eventSeverityId;
        EventSourceTypeId = eventSourceTypeId;
        EventCode = eventCode;
        Title = title;
        Summary = summary;
        DetectedAtUtc = detectedAtUtc;
        ReportedAtUtc = reportedAtUtc;
        ClosedAtUtc = closedAtUtc;
        ReportedByUserId = reportedByUserId;
        OwnerUserId = ownerUserId;
        OwningPersonId = owningPersonId;
        SourceReference = sourceReference;
        IsDrill = isDrill;
    }

    /// <summary>ReactorFleet.Unit real FK (ADR-022).</summary>
    public int UnitId { get; }

    /// <summary>Passport-only — Organization.Plant lives in OrganizationDb, a different physical database (ADR-022).</summary>
    public int? PlantId { get; }

    public EventTypeId EventTypeId { get; }

    public EventStatusId EventStatusId { get; }

    public EventSeverityId EventSeverityId { get; }

    public EventSourceTypeId EventSourceTypeId { get; }

    public string EventCode { get; }

    public string Title { get; }

    public string? Summary { get; }

    public DateTime DetectedAtUtc { get; }

    public DateTime ReportedAtUtc { get; }

    public DateTime? ClosedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-022).</summary>
    public int? ReportedByUserId { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-022).</summary>
    public int? OwnerUserId { get; }

    /// <summary>Passport-only — Organization.Person lives in OrganizationDb, a different physical database (ADR-022).</summary>
    public int? OwningPersonId { get; }

    public string? SourceReference { get; }

    public bool IsDrill { get; }

    public static OperationalEvent Create(
        OperationalEventId id, int unitId, EventTypeId eventTypeId, EventStatusId eventStatusId,
        EventSeverityId eventSeverityId, EventSourceTypeId eventSourceTypeId, string eventCode, string title,
        DateTime detectedAtUtc, DateTime reportedAtUtc, int? plantId = null, string? summary = null,
        DateTime? closedAtUtc = null, int? reportedByUserId = null, int? ownerUserId = null,
        int? owningPersonId = null, string? sourceReference = null, bool isDrill = false)
    {
        if (string.IsNullOrWhiteSpace(eventCode))
        {
            throw new ArgumentException("OperationalEvent code must not be empty.", nameof(eventCode));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("OperationalEvent title must not be empty.", nameof(title));
        }

        return new OperationalEvent(
            id, unitId, plantId, eventTypeId, eventStatusId, eventSeverityId, eventSourceTypeId, eventCode, title,
            summary, detectedAtUtc, reportedAtUtc, closedAtUtc, reportedByUserId, ownerUserId, owningPersonId,
            sourceReference, isDrill);
    }
}
