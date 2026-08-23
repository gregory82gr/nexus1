namespace Nexus1.DigitalTwin.Application;

public interface IActiveTwinFinder
{
    Task<IReadOnlyList<ActiveTwinDto>> GetActiveTwinsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Per-unit, unlike GetActiveTwinsAsync — added for the BFF's Plant 3D
    /// View screen. Named gap: this does not include divergence/sync-drift
    /// state. TwinDivergence links to TwinSnapshotId/SignalId, not directly
    /// to a unit; reaching a unit from a divergence requires TwinDivergence
    /// -> TwinSnapshot -> TwinRuntimeSession -> TwinModelVersion -> TwinModel.UnitId,
    /// a four-hop join no existing query performs. GetOpenDivergencesQuery
    /// (fleet-wide) doesn't carry a unit reference in its DTO either. Building
    /// that per-unit divergence join is a real, separate addition, not
    /// bundled into this one — flagged rather than fabricated or silently
    /// skipped.
    /// </summary>
    Task<IReadOnlyList<UnitTwinStateDto>> GetActiveTwinsForUnitAsync(int unitId, CancellationToken cancellationToken);
}
