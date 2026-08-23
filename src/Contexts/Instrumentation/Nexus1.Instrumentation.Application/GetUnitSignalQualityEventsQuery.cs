using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>Per ReactorFleet UnitId (int), unlike GetOpenSignalQualityEventsForUnitQuery (keyed by UnitCode string).</summary>
public sealed record GetUnitSignalQualityEventsQuery(int UnitId) : IQuery<IReadOnlyList<OpenSignalQualityEventDto>>;
