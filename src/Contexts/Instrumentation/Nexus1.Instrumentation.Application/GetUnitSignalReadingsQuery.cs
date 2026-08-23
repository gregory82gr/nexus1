using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>
/// Per ReactorFleet UnitId (int), unlike GetActiveHistorizedSignalsForUnitQuery
/// (keyed by UnitCode string) — added for route-shape consistency with every
/// other BFF endpoint (.../units/{id:int}), and because this query also
/// needs each signal's latest measurement, which GetActiveHistorizedSignalsForUnitQuery
/// alone doesn't include.
/// </summary>
public sealed record GetUnitSignalReadingsQuery(int UnitId) : IQuery<IReadOnlyList<UnitSignalReadingDto>>;
