using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.UnitTests;

public class UnitPowerSnapshotTests
{
    [Fact]
    public void Record_raises_exactly_one_UnitPowerRecorded_event_with_the_recorded_payload()
    {
        var unitId = new UnitId(7);
        var powerPercent = new PowerPercent(83.2m);
        var recordedAtUtc = new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

        var snapshot = UnitPowerSnapshot.Record(new UnitPowerSnapshotId(1), unitId, powerPercent, recordedAtUtc);

        Assert.Equal(unitId, snapshot.UnitId);
        Assert.Equal(powerPercent, snapshot.PowerPercent);
        Assert.Equal(recordedAtUtc, snapshot.RecordedAtUtc);

        var domainEvent = Assert.Single(snapshot.DomainEvents);
        var powerRecorded = Assert.IsType<UnitPowerRecorded>(domainEvent);
        Assert.Equal(unitId, powerRecorded.UnitId);
        Assert.Equal(powerPercent, powerRecorded.PowerPercent);
        Assert.Equal(recordedAtUtc, powerRecorded.RecordedAtUtc);
    }
}
