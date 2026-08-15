using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReactorFleet.Domain;

/// <summary>
/// Mirrors the Schema Atlas's append-only UnitPowerSnapshot table (no update
/// columns) — a write-once telemetry record, deliberately its own aggregate
/// rather than a child of Unit (ADR-003).
/// </summary>
public sealed class UnitPowerSnapshot : Entity<UnitPowerSnapshotId>, IAggregateRoot
{
    private UnitPowerSnapshot(UnitPowerSnapshotId id, UnitId unitId, PowerPercent powerPercent, DateTime recordedAtUtc)
        : base(id)
    {
        UnitId = unitId;
        PowerPercent = powerPercent;
        RecordedAtUtc = recordedAtUtc;
    }

    public UnitId UnitId { get; }

    public PowerPercent PowerPercent { get; }

    public DateTime RecordedAtUtc { get; }

    public static UnitPowerSnapshot Record(
        UnitPowerSnapshotId id, UnitId unitId, PowerPercent powerPercent, DateTime recordedAtUtc)
    {
        var snapshot = new UnitPowerSnapshot(id, unitId, powerPercent, recordedAtUtc);
        snapshot.AddDomainEvent(new UnitPowerRecorded(unitId, powerPercent, recordedAtUtc));
        return snapshot;
    }
}
