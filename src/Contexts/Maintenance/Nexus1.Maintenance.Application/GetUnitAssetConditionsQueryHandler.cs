using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

public sealed class GetUnitAssetConditionsQueryHandler(IAssetsByUnitFinder finder)
    : IQueryHandler<GetUnitAssetConditionsQuery, IReadOnlyList<UnitAssetConditionDto>>
{
    public async Task<Result<IReadOnlyList<UnitAssetConditionDto>>> Handle(
        GetUnitAssetConditionsQuery query, CancellationToken cancellationToken)
    {
        var conditions = await finder.GetAssetConditionsForUnitAsync(query.UnitId, cancellationToken);
        return Result<IReadOnlyList<UnitAssetConditionDto>>.Success(conditions);
    }
}
