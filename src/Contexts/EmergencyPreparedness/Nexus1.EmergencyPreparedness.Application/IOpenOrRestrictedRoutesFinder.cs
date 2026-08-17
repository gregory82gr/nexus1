namespace Nexus1.EmergencyPreparedness.Application;

public interface IOpenOrRestrictedRoutesFinder
{
    Task<IReadOnlyList<RouteCrossingZoneDto>> GetOpenOrRestrictedRoutesCrossingZonesAsync(CancellationToken cancellationToken);
}
