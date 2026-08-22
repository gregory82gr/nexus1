namespace Nexus1.Maintenance.Application;

public interface IAssetsByUnitFinder
{
    Task<IReadOnlyList<AssetByUnitDto>> GetAssetsByUnitAsync(CancellationToken cancellationToken);
}
