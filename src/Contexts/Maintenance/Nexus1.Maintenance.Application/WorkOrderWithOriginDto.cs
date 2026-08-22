namespace Nexus1.Maintenance.Application;

/// <summary>
/// Atlas C.9.5.2 query 3's own projection, now buildable as the atlas
/// literally specifies (ADR-022's reconnection): WorkOrderCode, the
/// linked event's EventCode, the linked incident action's Title (the
/// atlas's own query text says "ia.ActionCode", but IncidentAction has no
/// such column in its real DDL — see EventManagementIncidentActionReference's
/// own doc comment), and WorkOrder.Title. Previously (ADR-021) this DTO
/// carried the raw OriginOperationalEventId/OriginIncidentActionId
/// passport values instead, since EventManagement did not exist yet.
/// </summary>
public sealed record WorkOrderWithOriginDto(string WorkOrderCode, string? EventCode, string? IncidentActionTitle, string Title);
