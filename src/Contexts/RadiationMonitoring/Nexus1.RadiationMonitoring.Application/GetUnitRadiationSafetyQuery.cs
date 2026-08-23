using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Per-unit, unlike GetLatestReadingPerMonitorQuery/GetActiveRadiationZonesQuery (both fleet-wide).</summary>
public sealed record GetUnitRadiationSafetyQuery(int UnitId) : IQuery<UnitRadiationSafetyDto>;
