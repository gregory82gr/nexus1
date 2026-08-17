using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>Atlas C.9.5.2 query 5, verbatim: active (IsActive = 1) degradation records with mechanism, severity and trend point count, ordered by DetectedAtUtc desc.</summary>
public sealed record GetActiveDegradationCasesQuery : IQuery<IReadOnlyList<ActiveDegradationCaseDto>>;
