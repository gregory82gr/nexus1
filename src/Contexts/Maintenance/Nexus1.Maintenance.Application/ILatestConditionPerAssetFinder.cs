namespace Nexus1.Maintenance.Application;

public interface ILatestConditionPerAssetFinder
{
    Task<IReadOnlyList<LatestConditionDto>> GetLatestConditionPerAssetAsync(CancellationToken cancellationToken);
}
