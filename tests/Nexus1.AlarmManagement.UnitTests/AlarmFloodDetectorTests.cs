using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.UnitTests;

public class AlarmFloodDetectorTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Below_count_threshold_within_window_does_not_detect_a_flood()
    {
        var timestamps = new[] { NowUtc.AddSeconds(-10), NowUtc.AddSeconds(-5) };

        var detected = AlarmFloodDetector.ShouldDetectFlood(timestamps, NowUtc, countThreshold: 3, window: TimeSpan.FromSeconds(30));

        Assert.False(detected);
    }

    [Fact]
    public void At_count_threshold_within_window_detects_a_flood()
    {
        var timestamps = new[] { NowUtc.AddSeconds(-20), NowUtc.AddSeconds(-10), NowUtc.AddSeconds(-1) };

        var detected = AlarmFloodDetector.ShouldDetectFlood(timestamps, NowUtc, countThreshold: 3, window: TimeSpan.FromSeconds(30));

        Assert.True(detected);
    }

    [Fact]
    public void Timestamps_outside_the_window_are_not_counted()
    {
        var timestamps = new[] { NowUtc.AddMinutes(-5), NowUtc.AddMinutes(-4), NowUtc.AddSeconds(-1) };

        var detected = AlarmFloodDetector.ShouldDetectFlood(timestamps, NowUtc, countThreshold: 3, window: TimeSpan.FromSeconds(30));

        Assert.False(detected);
    }

    [Fact]
    public void Zero_or_negative_count_threshold_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AlarmFloodDetector.ShouldDetectFlood([], NowUtc, countThreshold: 0, window: TimeSpan.FromSeconds(30)));
    }
}
