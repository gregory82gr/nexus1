using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReactorFleet.Application;

public sealed class GetUnitByIdQueryHandler(IUnitFleetFinder finder)
    : IQueryHandler<GetUnitByIdQuery, UnitDetailDto>
{
    public async Task<Result<UnitDetailDto>> Handle(GetUnitByIdQuery query, CancellationToken cancellationToken)
    {
        var unit = await finder.GetDetailByIdAsync(query.UnitId, cancellationToken);

        return unit is null
            ? Result<UnitDetailDto>.Failure($"Unit {query.UnitId} does not exist.")
            : Result<UnitDetailDto>.Success(unit);
    }
}
