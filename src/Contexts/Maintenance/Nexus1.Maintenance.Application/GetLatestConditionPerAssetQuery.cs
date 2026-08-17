using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>Atlas C.9.5.2 query 4, verbatim: latest AssetCondition per Asset (OUTER APPLY TOP 1 by AssessedAtUtc desc), with condition grade, ordered by AssetCode.</summary>
public sealed record GetLatestConditionPerAssetQuery : IQuery<IReadOnlyList<LatestConditionDto>>;
