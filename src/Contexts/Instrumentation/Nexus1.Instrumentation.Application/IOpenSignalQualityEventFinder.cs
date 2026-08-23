namespace Nexus1.Instrumentation.Application;

public interface IOpenSignalQualityEventFinder
{
    Task<IReadOnlyList<OpenSignalQualityEventDto>> GetOpenByUnitCodeAsync(string unitCode, CancellationToken cancellationToken);

    /// <summary>
    /// Keyed by ReactorFleet UnitId (int) directly, filtering Signal.UnitId
    /// rather than joining through ReactorFleetUnitReference.Code — same
    /// route-shape-consistency reason as GetSignalReadingsForUnitAsync.
    /// Reuses OpenSignalQualityEventDto as-is; the projection shape is
    /// identical, only the key type differs. Added for the BFF's Model
    /// Analysis screen (ADR-030 follow-up) — this is Instrumentation's own
    /// real "verification" concept: is this unit's telemetry currently
    /// trustworthy, not a physics-model verification (that's DigitalTwin's
    /// divergence data, a separate context and a separate gap already
    /// recorded there).
    /// </summary>
    Task<IReadOnlyList<OpenSignalQualityEventDto>> GetOpenByUnitIdAsync(int unitId, CancellationToken cancellationToken);
}
