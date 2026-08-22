using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>Atlas C.9.5.2 query 1, adapted (see AssetByUnitDto): every non-deleted (IsDeleted = 0) asset with its unit, category and status, ordered by unit code then asset code.</summary>
public sealed record GetAssetsByUnitQuery : IQuery<IReadOnlyList<AssetByUnitDto>>;
