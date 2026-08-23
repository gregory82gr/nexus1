namespace Nexus1.RadiationMonitoring.Application;

public interface ILatestReadingPerMonitorFinder
{
    Task<IReadOnlyList<LatestRadiationReadingDto>> GetLatestReadingsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Per-unit, unlike GetLatestReadingsAsync (fleet-wide) — added for the
    /// BFF's Radiation &amp; Safety screen. RadiationMonitor.UnitId is a direct
    /// nullable FK to ReactorFleet.Unit, so this is a straightforward filter,
    /// not a multi-hop join. Includes monitors with no reading yet (null
    /// LatestValue etc.) rather than excluding them, unlike GetLatestReadingsAsync's
    /// "where latest != null" — a per-unit safety screen should show a sited
    /// monitor that simply hasn't reported yet, not hide it.
    /// </summary>
    Task<IReadOnlyList<UnitRadiationMonitorReadingDto>> GetLatestReadingsForUnitAsync(int unitId, CancellationToken cancellationToken);
}
