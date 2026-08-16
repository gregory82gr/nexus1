using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>
/// Atlas C.5.8 query 3 (EndedAtUtc IS NULL AND quality code IN ('BAD','STALE','UNCERTAIN'),
/// ordered by StartedAtUtc desc), narrowed to one unit to match this operation's own
/// "ForUnit" name — the atlas's own query 3 has no unit filter in its raw SQL despite
/// living under a sector organized by unit; ADR-019 names the operation
/// GetOpenSignalQualityEventsForUnitQuery, so a UnitCode join through Signal.UnitId is
/// added here rather than leaving the "ForUnit" name unfulfilled.
/// </summary>
public sealed record GetOpenSignalQualityEventsForUnitQuery(string UnitCode) : IQuery<IReadOnlyList<OpenSignalQualityEventDto>>;
