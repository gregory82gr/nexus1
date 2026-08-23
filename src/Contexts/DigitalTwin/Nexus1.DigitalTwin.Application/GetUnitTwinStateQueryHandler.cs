using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

public sealed class GetUnitTwinStateQueryHandler(IActiveTwinFinder finder)
    : IQueryHandler<GetUnitTwinStateQuery, IReadOnlyList<UnitTwinStateDto>>
{
    public async Task<Result<IReadOnlyList<UnitTwinStateDto>>> Handle(
        GetUnitTwinStateQuery query, CancellationToken cancellationToken)
    {
        var twins = await finder.GetActiveTwinsForUnitAsync(query.UnitId, cancellationToken);
        return Result<IReadOnlyList<UnitTwinStateDto>>.Success(twins);
    }
}
