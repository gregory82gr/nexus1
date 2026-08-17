using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>
/// The executable work package created from a plan, event, incident action
/// or manual request (atlas C.9.2, C.9.5.2 queries 2 and 3's subject).
///
/// UnitId is a real SQL FOREIGN KEY to ReactorFleet.Unit; AssetId is a real,
/// nullable internal FK to Maintenance.Asset (both ADR-021).
///
/// MaintenancePlanId/MaintenanceWindowId are passport-only — the planning
/// group (MaintenancePlan/MaintenanceWindow) is out of this phase's scope
/// (ADR-021), pointing at tables that could exist in a future step.
///
/// RequestedByUserId is passport-only (Security.ApplicationUser, different
/// physical database). AssignedTeamId/AssignedPersonId are passport-only
/// despite Organization existing — a direct, named consequence of ADR-021's
/// Option A/B persistence tradeoff (Organization.Team/Person live in
/// OrganizationDb, a different physical database than this sector's chosen
/// home, AlarmManagementDb).
///
/// OriginOperationalEventId/OriginIncidentActionId are REAL foreign keys
/// to EventManagement.OperationalEvent/IncidentAction as of ADR-022's own
/// reconnection decision. ADR-021 originally built them as passport-only
/// bigints with no enforced FK at all, because EventManagement (atlas
/// C.8) did not exist anywhere in this project at the time; once it was
/// built and confirmed to share this same physical database
/// (AlarmManagementDb), the reference was upgraded to a real FK via the
/// EventManagementOperationalEventReference/
/// EventManagementIncidentActionReference shadow entities.
///
/// RequestedAtUtc is the real business timestamp ("when was this work
/// requested") and is domain-modeled; CreatedAtUtc (present in the DDL
/// alongside CreatedBy/ModifiedAtUtc/ModifiedBy/RowVersion) is pure
/// row-insertion bookkeeping and is not modeled here. IsDeleted IS modeled:
/// C.9.5.2 query 2 (GetOpenWorkOrdersByUnitQuery) filters on
/// "IsDeleted = 0" as its own defining behavior.
/// </summary>
public sealed class WorkOrder : Entity<WorkOrderId>, IAggregateRoot
{
    private WorkOrder(
        WorkOrderId id, int unitId, AssetId? assetId, int? maintenancePlanId, int? maintenanceWindowId,
        WorkOrderTypeId workOrderTypeId, WorkOrderStatusId workOrderStatusId, WorkPriorityId workPriorityId,
        string workOrderCode, string title, string? description, DateTime requestedAtUtc, DateTime? dueAtUtc,
        DateTime? startedAtUtc, DateTime? completedAtUtc, DateTime? closedAtUtc, int? requestedByUserId,
        int? assignedTeamId, int? assignedPersonId, long? originOperationalEventId, long? originIncidentActionId,
        bool isDeleted)
        : base(id)
    {
        UnitId = unitId;
        AssetId = assetId;
        MaintenancePlanId = maintenancePlanId;
        MaintenanceWindowId = maintenanceWindowId;
        WorkOrderTypeId = workOrderTypeId;
        WorkOrderStatusId = workOrderStatusId;
        WorkPriorityId = workPriorityId;
        WorkOrderCode = workOrderCode;
        Title = title;
        Description = description;
        RequestedAtUtc = requestedAtUtc;
        DueAtUtc = dueAtUtc;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        ClosedAtUtc = closedAtUtc;
        RequestedByUserId = requestedByUserId;
        AssignedTeamId = assignedTeamId;
        AssignedPersonId = assignedPersonId;
        OriginOperationalEventId = originOperationalEventId;
        OriginIncidentActionId = originIncidentActionId;
        IsDeleted = isDeleted;
    }

    /// <summary>ReactorFleet.Unit real FK (ADR-021).</summary>
    public int UnitId { get; }

    /// <summary>Real, nullable internal FK to Maintenance.Asset.</summary>
    public AssetId? AssetId { get; }

    /// <summary>Passport-only — Maintenance.MaintenancePlan is out of scope this phase (ADR-021).</summary>
    public int? MaintenancePlanId { get; }

    /// <summary>Passport-only — Maintenance.MaintenanceWindow is out of scope this phase (ADR-021).</summary>
    public int? MaintenanceWindowId { get; }

    public WorkOrderTypeId WorkOrderTypeId { get; }

    public WorkOrderStatusId WorkOrderStatusId { get; }

    public WorkPriorityId WorkPriorityId { get; }

    public string WorkOrderCode { get; }

    public string Title { get; }

    public string? Description { get; }

    public DateTime RequestedAtUtc { get; }

    public DateTime? DueAtUtc { get; }

    public DateTime? StartedAtUtc { get; }

    public DateTime? CompletedAtUtc { get; }

    public DateTime? ClosedAtUtc { get; }

    /// <summary>Passport-only — Security.ApplicationUser (ADR-021).</summary>
    public int? RequestedByUserId { get; }

    /// <summary>Passport-only — Organization.Team lives in OrganizationDb, a different physical database (ADR-021).</summary>
    public int? AssignedTeamId { get; }

    /// <summary>Passport-only — Organization.Person lives in OrganizationDb, a different physical database (ADR-021).</summary>
    public int? AssignedPersonId { get; }

    /// <summary>Real FK to EventManagement.OperationalEvent, via a shadow entity (ADR-022's reconnection decision; originally passport-only under ADR-021).</summary>
    public long? OriginOperationalEventId { get; }

    /// <summary>Real FK to EventManagement.IncidentAction, via a shadow entity (ADR-022's reconnection decision; originally passport-only under ADR-021).</summary>
    public long? OriginIncidentActionId { get; }

    public bool IsDeleted { get; }

    public static WorkOrder Create(
        WorkOrderId id, int unitId, WorkOrderTypeId workOrderTypeId, WorkOrderStatusId workOrderStatusId,
        WorkPriorityId workPriorityId, string workOrderCode, string title, DateTime requestedAtUtc,
        AssetId? assetId = null, int? maintenancePlanId = null, int? maintenanceWindowId = null,
        string? description = null, DateTime? dueAtUtc = null, DateTime? startedAtUtc = null,
        DateTime? completedAtUtc = null, DateTime? closedAtUtc = null, int? requestedByUserId = null,
        int? assignedTeamId = null, int? assignedPersonId = null, long? originOperationalEventId = null,
        long? originIncidentActionId = null)
    {
        if (string.IsNullOrWhiteSpace(workOrderCode))
        {
            throw new ArgumentException("WorkOrder code must not be empty.", nameof(workOrderCode));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("WorkOrder title must not be empty.", nameof(title));
        }

        return new WorkOrder(
            id, unitId, assetId, maintenancePlanId, maintenanceWindowId, workOrderTypeId, workOrderStatusId,
            workPriorityId, workOrderCode, title, description, requestedAtUtc, dueAtUtc, startedAtUtc, completedAtUtc,
            closedAtUtc, requestedByUserId, assignedTeamId, assignedPersonId, originOperationalEventId,
            originIncidentActionId, isDeleted: false);
    }
}
