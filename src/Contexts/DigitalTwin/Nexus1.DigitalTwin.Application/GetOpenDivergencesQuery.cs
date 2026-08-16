using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

/// <summary>Atlas C.6.8 query 3, verbatim: open/acknowledged divergences with signal tag, model variable, values, severity, status, ordered by DetectedAtUtc desc.</summary>
public sealed record GetOpenDivergencesQuery : IQuery<IReadOnlyList<OpenDivergenceDto>>;
