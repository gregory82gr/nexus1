using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

/// <summary>Per-unit, unlike GetActiveTwinsForFleetQuery (fleet-wide).</summary>
public sealed record GetUnitTwinStateQuery(int UnitId) : IQuery<IReadOnlyList<UnitTwinStateDto>>;
