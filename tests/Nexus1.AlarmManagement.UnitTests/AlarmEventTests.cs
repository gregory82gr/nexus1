using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.UnitTests;

public class AlarmEventTests
{
    private static AlarmEvent RaiseSample() => AlarmEvent.Raise(
        new AlarmEventId(1), new AlarmDefinitionId(1), new UnitId(1), AlarmSeverity.Critical,
        new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc), 150m, 100m, "HIGH-POWER breached: 150 >= 100.");

    [Fact]
    public void Raise_starts_in_active_state_and_raises_AlarmRaised_event()
    {
        var alarmEvent = RaiseSample();

        Assert.Equal(AlarmState.Active, alarmEvent.State);
        var raised = Assert.IsType<AlarmRaised>(Assert.Single(alarmEvent.DomainEvents));
        Assert.Equal(alarmEvent.Id, raised.AlarmEventId);
        Assert.Equal(alarmEvent.AlarmDefinitionId, raised.AlarmDefinitionId);
        Assert.Equal(alarmEvent.UnitId, raised.UnitId);
        Assert.Equal(AlarmSeverity.Critical, raised.Severity);
    }

    [Fact]
    public void Acknowledge_from_active_transitions_to_acknowledged_and_raises_event()
    {
        var alarmEvent = RaiseSample();
        alarmEvent.ClearDomainEvents();
        var userId = new UserId(Guid.NewGuid());
        var acknowledgedAtUtc = new DateTime(2026, 8, 15, 12, 5, 0, DateTimeKind.Utc);

        alarmEvent.Acknowledge(userId, acknowledgedAtUtc);

        Assert.Equal(AlarmState.Acknowledged, alarmEvent.State);
        Assert.Equal(userId, alarmEvent.AcknowledgedBy);
        Assert.Equal(acknowledgedAtUtc, alarmEvent.AcknowledgedAtUtc);
        var acknowledged = Assert.IsType<AlarmAcknowledged>(Assert.Single(alarmEvent.DomainEvents));
        Assert.Equal(alarmEvent.Id, acknowledged.AlarmEventId);
        Assert.Equal(userId, acknowledged.AcknowledgedBy);
    }

    [Fact]
    public void Acknowledge_twice_throws_on_the_second_call()
    {
        var alarmEvent = RaiseSample();
        alarmEvent.Acknowledge(new UserId(Guid.NewGuid()), DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() => alarmEvent.Acknowledge(new UserId(Guid.NewGuid()), DateTime.UtcNow));
    }
}
