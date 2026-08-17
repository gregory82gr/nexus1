using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

/// <summary>Atlas C.13.5.2 query 3, verbatim: one row per monitor, most recent RadiationReading, with engineering-unit symbol and quality code.</summary>
public sealed record GetLatestReadingPerMonitorQuery : IQuery<IReadOnlyList<LatestRadiationReadingDto>>;
