namespace Nexus1.RadiationMonitoring.Application;

public interface IActiveRadiationZonesFinder
{
    Task<IReadOnlyList<ActiveRadiationZoneDto>> GetActiveRadiationZonesAsync(CancellationToken cancellationToken);
}
