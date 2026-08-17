using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

public sealed class GetOpenOrRestrictedRoutesCrossingZonesQueryHandler(IOpenOrRestrictedRoutesFinder finder)
    : IQueryHandler<GetOpenOrRestrictedRoutesCrossingZonesQuery, IReadOnlyList<RouteCrossingZoneDto>>
{
    public async Task<Result<IReadOnlyList<RouteCrossingZoneDto>>> Handle(GetOpenOrRestrictedRoutesCrossingZonesQuery query, CancellationToken cancellationToken)
    {
        var routes = await finder.GetOpenOrRestrictedRoutesCrossingZonesAsync(cancellationToken);
        return Result<IReadOnlyList<RouteCrossingZoneDto>>.Success(routes);
    }
}
