using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>
/// Connects an OperationalEvent to one or more alarm events that triggered
/// or supported it (atlas C.8.1/C.8.4.4, query 1's subject). OperationalEventId
/// is a real internal FK; AlarmEventId is a real SQL FOREIGN KEY into
/// AlarmManagement.AlarmEvent via the AlarmManagementAlarmEventReference
/// shadow-entity technique — the first-ever shadow reference into
/// AlarmManagement for this codebase (ADR-022). Unique on
/// (OperationalEventId, AlarmEventId) per the real DDL.
/// </summary>
public sealed class EventAlarmLink : Entity<EventAlarmLinkId>, IAggregateRoot
{
    private EventAlarmLink(EventAlarmLinkId id, OperationalEventId operationalEventId, long alarmEventId, string linkRole, string? note)
        : base(id)
    {
        OperationalEventId = operationalEventId;
        AlarmEventId = alarmEventId;
        LinkRole = linkRole;
        Note = note;
    }

    /// <summary>Real internal FK to OperationalEvent — typed to match OperationalEvent's own strongly-typed Id (EF requires FK/principal-key CLR types to agree once a value converter is in play).</summary>
    public OperationalEventId OperationalEventId { get; }

    /// <summary>AlarmManagement.AlarmEvent real FK (ADR-022) — plain long, matching the shadow reference's primitive key.</summary>
    public long AlarmEventId { get; }

    public string LinkRole { get; }

    public string? Note { get; }

    public static EventAlarmLink Create(
        EventAlarmLinkId id, long operationalEventId, long alarmEventId, string linkRole = "SUPPORTING", string? note = null)
    {
        if (string.IsNullOrWhiteSpace(linkRole))
        {
            throw new ArgumentException("EventAlarmLink role must not be empty.", nameof(linkRole));
        }

        return new EventAlarmLink(id, new OperationalEventId(operationalEventId), alarmEventId, linkRole, note);
    }
}
