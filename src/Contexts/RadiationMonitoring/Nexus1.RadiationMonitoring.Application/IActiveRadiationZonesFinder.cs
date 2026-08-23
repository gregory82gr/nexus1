namespace Nexus1.RadiationMonitoring.Application;

public interface IActiveRadiationZonesFinder
{
    Task<IReadOnlyList<ActiveRadiationZoneDto>> GetActiveRadiationZonesAsync(CancellationToken cancellationToken);

    /// <summary>Per-unit, unlike GetActiveRadiationZonesAsync (fleet-wide) — added for the BFF's Radiation &amp; Safety screen.</summary>
    Task<IReadOnlyList<UnitRadiationZoneDto>> GetActiveRadiationZonesForUnitAsync(int unitId, CancellationToken cancellationToken);
}
