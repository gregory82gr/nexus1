using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReactorFleet.Application;

public sealed class GetUnitsQueryHandler(IUnitFleetFinder finder)
    : IQueryHandler<GetUnitsQuery, IReadOnlyList<UnitSummaryDto>>
{
    public async Task<Result<IReadOnlyList<UnitSummaryDto>>> Handle(GetUnitsQuery query, CancellationToken cancellationToken)
    {
        var units = await finder.GetAllSummariesAsync(cancellationToken);
        return Result<IReadOnlyList<UnitSummaryDto>>.Success(units);
    }
}
