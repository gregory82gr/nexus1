using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>Atlas C.5.8 query 1, verbatim: active historized signals for one unit, ordered by Tag.</summary>
public sealed record GetActiveHistorizedSignalsForUnitQuery(string UnitCode) : IQuery<IReadOnlyList<ActiveHistorizedSignalDto>>;
