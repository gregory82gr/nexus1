using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

public sealed class GetActiveEngineeringUnitsQueryHandler(IEngineeringUnitFinder engineeringUnitFinder)
    : IQueryHandler<GetActiveEngineeringUnitsQuery, IReadOnlyList<EngineeringUnitDto>>
{
    public async Task<Result<IReadOnlyList<EngineeringUnitDto>>> Handle(
        GetActiveEngineeringUnitsQuery query, CancellationToken cancellationToken)
    {
        var units = await engineeringUnitFinder.GetActiveAsync(cancellationToken);

        IReadOnlyList<EngineeringUnitDto> dtos = units
            .Select(u => new EngineeringUnitDto(u.Id.Value, u.Symbol, u.Name, u.QuantityType.ToString()))
            .ToList();

        return Result<IReadOnlyList<EngineeringUnitDto>>.Success(dtos);
    }
}
