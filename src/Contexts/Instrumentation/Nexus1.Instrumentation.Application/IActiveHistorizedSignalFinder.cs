namespace Nexus1.Instrumentation.Application;

public interface IActiveHistorizedSignalFinder
{
    /// <summary>Atlas C.5.8 query 1: WHERE ru.Code = @unitCode AND s.IsHistorized = 1 AND s.IsDeleted = 0, ORDER BY s.Tag.</summary>
    Task<IReadOnlyList<ActiveHistorizedSignalDto>> GetByUnitCodeAsync(string unitCode, CancellationToken cancellationToken);

    /// <summary>
    /// Keyed by ReactorFleet UnitId (int) directly, filtering Signal.UnitId
    /// rather than joining through ReactorFleetUnitReference.Code — and
    /// includes each signal's latest measurement, which GetByUnitCodeAsync
    /// alone doesn't. Added for the BFF's Reactor sub-screens (ADR-030
    /// follow-up). Includes a signal with zero measurements yet (null
    /// reading fields) rather than excluding it, same reasoning as every
    /// other per-unit "latest X" finder added so far.
    /// </summary>
    Task<IReadOnlyList<UnitSignalReadingDto>> GetSignalReadingsForUnitAsync(int unitId, CancellationToken cancellationToken);
}
