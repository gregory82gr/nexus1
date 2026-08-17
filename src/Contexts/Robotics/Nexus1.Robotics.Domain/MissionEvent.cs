using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>
/// An append-only timeline entry for a Mission (ADR-023, atlas query 3's own
/// subject) — mirrors EventManagement.EventTimelineEntry's shape, a mission
/// is itself a small event-sourced record. MissionId is a real internal FK
/// and NOT NULL; RobotId is a real internal FK and nullable (not every
/// mission event has an acting robot). RecordedByUserId is passport-only —
/// Security.ApplicationUser, a different physical database. No audit
/// columns — lean, append-only chronology.
///
/// MissionTaskId is deliberately omitted entirely — out of scope for this
/// pass, its target table does not exist in this codebase (ADR-023).
/// </summary>
public sealed class MissionEvent : Entity<MissionEventId>, IAggregateRoot
{
    private MissionEvent(
        MissionEventId id, MissionId missionId, RobotId? robotId, DateTime occurredAtUtc, string eventCode,
        string title, string? detail, bool isFault, int? recordedByUserId)
        : base(id)
    {
        MissionId = missionId;
        RobotId = robotId;
        OccurredAtUtc = occurredAtUtc;
        EventCode = eventCode;
        Title = title;
        Detail = detail;
        IsFault = isFault;
        RecordedByUserId = recordedByUserId;
    }

    /// <summary>Real internal FK to Mission — typed to match Mission's own strongly-typed Id.</summary>
    public MissionId MissionId { get; }

    /// <summary>Real internal FK to Robot, nullable — not every mission event has an acting robot.</summary>
    public RobotId? RobotId { get; }

    public DateTime OccurredAtUtc { get; }

    public string EventCode { get; }

    public string Title { get; }

    public string? Detail { get; }

    public bool IsFault { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-023).</summary>
    public int? RecordedByUserId { get; }

    public static MissionEvent Create(
        MissionEventId id, long missionId, RobotId? robotId, DateTime occurredAtUtc, string eventCode, string title,
        string? detail = null, bool isFault = false, int? recordedByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(eventCode))
        {
            throw new ArgumentException("MissionEvent event code must not be empty.", nameof(eventCode));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("MissionEvent title must not be empty.", nameof(title));
        }

        return new MissionEvent(id, new MissionId(missionId), robotId, occurredAtUtc, eventCode, title, detail, isFault, recordedByUserId);
    }
}
