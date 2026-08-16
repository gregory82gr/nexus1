using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.UnitTests;

public class SignalQualityEventTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime StartedAtUtc = new(2026, 8, 16, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_opens_the_event_with_no_end_date()
    {
        var qualityEvent = SignalQualityEvent.Create(
            new SignalQualityEventId(1), new SignalId(1), new SignalQualityId(1), StartedAtUtc, NowUtc, reasonCode: "SENSOR_FAULT");

        Assert.Null(qualityEvent.EndedAtUtc);
        Assert.Equal("SENSOR_FAULT", qualityEvent.ReasonCode);
    }

    [Fact]
    public void Close_with_valid_end_date_closes_the_event()
    {
        var qualityEvent = SignalQualityEvent.Create(
            new SignalQualityEventId(1), new SignalId(1), new SignalQualityId(1), StartedAtUtc, NowUtc);

        qualityEvent.Close(StartedAtUtc.AddHours(2));

        Assert.Equal(StartedAtUtc.AddHours(2), qualityEvent.EndedAtUtc);
    }

    [Fact]
    public void Close_with_reason_code_overwrites_the_reason_recorded_at_open()
    {
        var qualityEvent = SignalQualityEvent.Create(
            new SignalQualityEventId(1), new SignalId(1), new SignalQualityId(1), StartedAtUtc, NowUtc, reasonCode: "SENSOR_FAULT");

        qualityEvent.Close(StartedAtUtc.AddHours(2), reasonCode: "REPAIRED");

        Assert.Equal("REPAIRED", qualityEvent.ReasonCode);
    }

    [Fact]
    public void Close_without_reason_code_keeps_the_reason_recorded_at_open()
    {
        var qualityEvent = SignalQualityEvent.Create(
            new SignalQualityEventId(1), new SignalId(1), new SignalQualityId(1), StartedAtUtc, NowUtc, reasonCode: "SENSOR_FAULT");

        qualityEvent.Close(StartedAtUtc.AddHours(2));

        Assert.Equal("SENSOR_FAULT", qualityEvent.ReasonCode);
    }

    [Fact]
    public void Close_with_end_date_equal_to_start_date_throws()
    {
        var qualityEvent = SignalQualityEvent.Create(
            new SignalQualityEventId(1), new SignalId(1), new SignalQualityId(1), StartedAtUtc, NowUtc);

        Assert.Throws<ArgumentException>(() => qualityEvent.Close(StartedAtUtc));
    }

    [Fact]
    public void Close_with_end_date_before_start_date_throws()
    {
        var qualityEvent = SignalQualityEvent.Create(
            new SignalQualityEventId(1), new SignalId(1), new SignalQualityId(1), StartedAtUtc, NowUtc);

        Assert.Throws<ArgumentException>(() => qualityEvent.Close(StartedAtUtc.AddHours(-1)));
    }
}
