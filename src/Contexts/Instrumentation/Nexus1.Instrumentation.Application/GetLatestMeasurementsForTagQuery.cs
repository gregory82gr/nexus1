using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>Atlas C.5.8 query 2, verbatim: the most recent measurements for a tag, newest first. Count defaults to the atlas's own TOP (10).</summary>
public sealed record GetLatestMeasurementsForTagQuery(string Tag, int Count = 10) : IQuery<IReadOnlyList<LatestMeasurementDto>>;
