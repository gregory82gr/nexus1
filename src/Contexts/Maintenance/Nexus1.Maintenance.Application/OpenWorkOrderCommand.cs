using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>WorkOrder's defining behavior (ADR-021): opens a new executable work package against a unit, optionally against a specific asset.</summary>
public sealed record OpenWorkOrderCommand(
    int UnitId, int WorkOrderTypeId, int WorkOrderStatusId, int WorkPriorityId, string WorkOrderCode, string Title,
    DateTime RequestedAtUtc, int? AssetId = null, int? MaintenancePlanId = null, int? MaintenanceWindowId = null,
    string? Description = null, DateTime? DueAtUtc = null, int? RequestedByUserId = null, int? AssignedTeamId = null,
    int? AssignedPersonId = null, long? OriginOperationalEventId = null, long? OriginIncidentActionId = null)
    : ICommand<long>;
