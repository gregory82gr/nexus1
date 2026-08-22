using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Captures the model state at one instant (atlas C.6.1/C.6.2). Built as a
/// pair with TwinSnapshotValue deliberately: a TwinSnapshot alone (no
/// values) would be an empty shell that "freezes" nothing (ADR-020).
///
/// Leaner audit shape than most tables in this sector — the atlas DDL gives
/// this table only CreatedAtUtc (no CreatedBy/ModifiedAtUtc/ModifiedBy/
/// IsDeleted/RowVersion), verified directly against C.6.4.5. SnapshotAtUtc
/// is the real business timestamp (when the state was captured); CreatedAtUtc
/// is pure row-insertion bookkeeping and is not modeled here.
/// </summary>
public sealed class TwinSnapshot : Entity<TwinSnapshotId>, IAggregateRoot
{
    private TwinSnapshot(
        TwinSnapshotId id, TwinRuntimeSessionId twinRuntimeSessionId, SnapshotReasonId snapshotReasonId,
        DateTime snapshotAtUtc, long? timeStepIndex, byte[]? stateVectorHash, string? summaryJson)
        : base(id)
    {
        TwinRuntimeSessionId = twinRuntimeSessionId;
        SnapshotReasonId = snapshotReasonId;
        SnapshotAtUtc = snapshotAtUtc;
        TimeStepIndex = timeStepIndex;
        StateVectorHash = stateVectorHash;
        SummaryJson = summaryJson;
    }

    public TwinRuntimeSessionId TwinRuntimeSessionId { get; }

    public SnapshotReasonId SnapshotReasonId { get; }

    public DateTime SnapshotAtUtc { get; }

    public long? TimeStepIndex { get; }

    public byte[]? StateVectorHash { get; }

    public string? SummaryJson { get; }

    public static TwinSnapshot Create(
        TwinSnapshotId id, TwinRuntimeSessionId twinRuntimeSessionId, SnapshotReasonId snapshotReasonId,
        DateTime snapshotAtUtc, long? timeStepIndex = null, byte[]? stateVectorHash = null, string? summaryJson = null)
    {
        return new TwinSnapshot(id, twinRuntimeSessionId, snapshotReasonId, snapshotAtUtc, timeStepIndex, stateVectorHash, summaryJson);
    }
}
