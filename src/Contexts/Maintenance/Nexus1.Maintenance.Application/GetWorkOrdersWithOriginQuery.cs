using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>
/// Atlas C.9.5.2 query 3, adapted (see WorkOrderWithOriginDto for why):
/// work orders where either OriginOperationalEventId or
/// OriginIncidentActionId is not null, returning the raw passport values
/// with no join — EventManagement.OperationalEvent/IncidentAction do not
/// exist anywhere in this project yet (ADR-021).
/// </summary>
public sealed record GetWorkOrdersWithOriginQuery : IQuery<IReadOnlyList<WorkOrderWithOriginDto>>;
