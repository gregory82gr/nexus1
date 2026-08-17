using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>Atlas verification query 3, verbatim: EvacuationRoute JOIN RouteStatus, JOIN EvacuationRouteZone, JOIN RadiationMonitoring.RadiationZone, WHERE RouteStatus.Code IN ('OPEN','RESTRICTED').</summary>
public sealed record GetOpenOrRestrictedRoutesCrossingZonesQuery : IQuery<IReadOnlyList<RouteCrossingZoneDto>>;
