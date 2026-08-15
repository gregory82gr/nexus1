using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.UnitTests;

public class AlarmFloodTests
{
    private static readonly DateTime StartedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Detect_starts_in_detected_status_and_raises_event()
    {
        var flood = AlarmFlood.Detect(new AlarmFloodId(1), new UnitId(1), StartedAtUtc);

        Assert.Equal(AlarmFloodStatus.Detected, flood.Status);
        Assert.Empty(flood.MemberAlarmEventIds);
        var detected = Assert.IsType<AlarmFloodDetected>(Assert.Single(flood.DomainEvents));
        Assert.Equal(flood.Id, detected.AlarmFloodId);
        Assert.Equal(flood.UnitId, detected.UnitId);
        Assert.Equal(StartedAtUtc, detected.StartedAtUtc);
    }

    [Fact]
    public void AddMember_within_window_succeeds()
    {
        var flood = AlarmFlood.Detect(new AlarmFloodId(1), new UnitId(1), StartedAtUtc);

        flood.AddMember(new AlarmEventId(1), StartedAtUtc.AddSeconds(30), TimeSpan.FromMinutes(1));

        Assert.Equal(new AlarmEventId(1), Assert.Single(flood.MemberAlarmEventIds));
    }

    [Fact]
    public void AddMember_before_the_floods_start_throws()
    {
        var flood = AlarmFlood.Detect(new AlarmFloodId(1), new UnitId(1), StartedAtUtc);

        Assert.Throws<InvalidOperationException>(() =>
            flood.AddMember(new AlarmEventId(1), StartedAtUtc.AddSeconds(-1), TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void AddMember_after_the_window_throws()
    {
        var flood = AlarmFlood.Detect(new AlarmFloodId(1), new UnitId(1), StartedAtUtc);

        Assert.Throws<InvalidOperationException>(() =>
            flood.AddMember(new AlarmEventId(1), StartedAtUtc.AddMinutes(1).AddSeconds(1), TimeSpan.FromMinutes(1)));
    }
}
