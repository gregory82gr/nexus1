using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

public sealed class GetAssetsByUnitQueryHandler(IAssetsByUnitFinder finder)
    : IQueryHandler<GetAssetsByUnitQuery, IReadOnlyList<AssetByUnitDto>>
{
    public async Task<Result<IReadOnlyList<AssetByUnitDto>>> Handle(GetAssetsByUnitQuery query, CancellationToken cancellationToken)
    {
        var assets = await finder.GetAssetsByUnitAsync(cancellationToken);
        return Result<IReadOnlyList<AssetByUnitDto>>.Success(assets);
    }
}
