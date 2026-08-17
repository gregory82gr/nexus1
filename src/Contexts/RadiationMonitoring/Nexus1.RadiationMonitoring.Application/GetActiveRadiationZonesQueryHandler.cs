using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

public sealed class GetActiveRadiationZonesQueryHandler(IActiveRadiationZonesFinder finder)
    : IQueryHandler<GetActiveRadiationZonesQuery, IReadOnlyList<ActiveRadiationZoneDto>>
{
    public async Task<Result<IReadOnlyList<ActiveRadiationZoneDto>>> Handle(GetActiveRadiationZonesQuery query, CancellationToken cancellationToken)
    {
        var zones = await finder.GetActiveRadiationZonesAsync(cancellationToken);
        return Result<IReadOnlyList<ActiveRadiationZoneDto>>.Success(zones);
    }
}
