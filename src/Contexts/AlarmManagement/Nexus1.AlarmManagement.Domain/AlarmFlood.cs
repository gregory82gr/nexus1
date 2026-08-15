using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// Groups AlarmEvents by reference (ArrivalOrder in the atlas), never owns
/// them — matches the book's AlarmFloodMember pattern.
/// </summary>
public sealed class AlarmFlood : Entity<AlarmFloodId>, IAggregateRoot
{
    private readonly List<AlarmEventId> _memberAlarmEventIds = [];

    private AlarmFlood(AlarmFloodId id, UnitId unitId, DateTime startedAtUtc)
        : base(id)
    {
        UnitId = unitId;
        Status = AlarmFloodStatus.Detected;
        StartedAtUtc = startedAtUtc;
    }

    public UnitId UnitId { get; }

    public AlarmFloodStatus Status { get; private set; }

    public DateTime StartedAtUtc { get; }

    public DateTime? EndedAtUtc { get; private set; }

    public IReadOnlyList<AlarmEventId> MemberAlarmEventIds => _memberAlarmEventIds.AsReadOnly();

    public static AlarmFlood Detect(AlarmFloodId id, UnitId unitId, DateTime startedAtUtc)
    {
        var flood = new AlarmFlood(id, unitId, startedAtUtc);
        flood.AddDomainEvent(new AlarmFloodDetected(id, unitId, startedAtUtc));
        return flood;
    }

    /// <summary>
    /// Enforces the book's stated rule: "a flood cannot contain alarms from
    /// unrelated time windows." `window` is caller-supplied rather than a
    /// hardcoded default — neither source states a number (ADR-004).
    /// </summary>
    public void AddMember(AlarmEventId alarmEventId, DateTime raisedAtUtc, TimeSpan window)
    {
        if (raisedAtUtc < StartedAtUtc || raisedAtUtc > StartedAtUtc + window)
        {
            throw new InvalidOperationException(
                $"Alarm event raised at {raisedAtUtc:O} falls outside this flood's window " +
                $"[{StartedAtUtc:O}, {StartedAtUtc + window:O}].");
        }

        _memberAlarmEventIds.Add(alarmEventId);
    }
}
