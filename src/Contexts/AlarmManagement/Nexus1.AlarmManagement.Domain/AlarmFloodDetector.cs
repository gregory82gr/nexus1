namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// Stateless policy over already-materialized alarm timestamps — pure domain
/// logic, no persistence/subscription plumbing. countThreshold/window are
/// required parameters, not defaults: neither source material specifies a
/// number, so none is invented here (ADR-004).
/// </summary>
public static class AlarmFloodDetector
{
    public static bool ShouldDetectFlood(
        IReadOnlyList<DateTime> recentAlarmRaisedAtUtc, DateTime nowUtc, int countThreshold, TimeSpan window)
    {
        if (countThreshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(countThreshold), countThreshold, "Count threshold must be at least 1.");
        }

        var count = recentAlarmRaisedAtUtc.Count(raisedAtUtc => nowUtc - raisedAtUtc <= window && raisedAtUtc <= nowUtc);
        return count >= countThreshold;
    }
}
