using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

public sealed class GetLatestConditionPerAssetQueryHandler(ILatestConditionPerAssetFinder finder)
    : IQueryHandler<GetLatestConditionPerAssetQuery, IReadOnlyList<LatestConditionDto>>
{
    public async Task<Result<IReadOnlyList<LatestConditionDto>>> Handle(GetLatestConditionPerAssetQuery query, CancellationToken cancellationToken)
    {
        var conditions = await finder.GetLatestConditionPerAssetAsync(cancellationToken);
        return Result<IReadOnlyList<LatestConditionDto>>.Success(conditions);
    }
}
