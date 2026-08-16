using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class TwinSnapshotTests
{
    private static readonly DateTime SnapshotAtUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_minimal_fields_succeeds()
    {
        var snapshot = TwinSnapshot.Create(new TwinSnapshotId(1), new TwinRuntimeSessionId(1), new SnapshotReasonId(1), SnapshotAtUtc);

        Assert.Equal(SnapshotAtUtc, snapshot.SnapshotAtUtc);
        Assert.Null(snapshot.TimeStepIndex);
    }

    [Fact]
    public void Create_with_time_step_index_records_it()
    {
        var snapshot = TwinSnapshot.Create(
            new TwinSnapshotId(1), new TwinRuntimeSessionId(1), new SnapshotReasonId(1), SnapshotAtUtc, timeStepIndex: 42);

        Assert.Equal(42, snapshot.TimeStepIndex);
    }
}
