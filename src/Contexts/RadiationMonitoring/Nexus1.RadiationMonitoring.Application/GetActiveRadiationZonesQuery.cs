using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Atlas C.13.5.2 query 1, verbatim: active radiation zones (IsDeleted = 0) joined to their classification, status and (left) unit.</summary>
public sealed record GetActiveRadiationZonesQuery : IQuery<IReadOnlyList<ActiveRadiationZoneDto>>;
