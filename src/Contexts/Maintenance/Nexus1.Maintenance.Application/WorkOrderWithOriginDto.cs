namespace Nexus1.Maintenance.Application;

/// <summary>
/// Atlas C.9.5.2 query 3 projection, adapted: WorkOrderCode/Title plus the
/// raw OriginOperationalEventId/OriginIncidentActionId passport values
/// themselves, rather than joining EventManagement.OperationalEvent/
/// IncidentAction (which don't exist in this database — EventManagement,
/// atlas C.8, is not built anywhere in this project yet). Proves the real,
/// buildable half of the atlas's own intent: that WorkOrder correctly
/// records which event/incident triggered it, without fabricating a join
/// against tables this project hasn't built (ADR-021).
/// </summary>
public sealed record WorkOrderWithOriginDto(string WorkOrderCode, string Title, long? OriginOperationalEventId, long? OriginIncidentActionId);
