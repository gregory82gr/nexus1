using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>
/// Connects an OperationalEvent to an alarm flood window (atlas C.8.1/C.8.4.4,
/// query 1's subject). OperationalEventId is a real internal FK; AlarmFloodId
/// is a real SQL FOREIGN KEY into AlarmManagement.AlarmFlood via the
/// AlarmManagementAlarmFloodReference shadow-entity technique — the second
/// first-ever shadow reference into AlarmManagement for this codebase
/// (ADR-022). Unique on (OperationalEventId, AlarmFloodId) per the real DDL.
/// </summary>
public sealed class EventFloodLink : Entity<EventFloodLinkId>, IAggregateRoot
{
    private EventFloodLink(EventFloodLinkId id, OperationalEventId operationalEventId, long alarmFloodId, string linkRole, string? note)
        : base(id)
    {
        OperationalEventId = operationalEventId;
        AlarmFloodId = alarmFloodId;
        LinkRole = linkRole;
        Note = note;
    }

    /// <summary>Real internal FK to OperationalEvent — typed to match OperationalEvent's own strongly-typed Id (EF requires FK/principal-key CLR types to agree once a value converter is in play).</summary>
    public OperationalEventId OperationalEventId { get; }

    /// <summary>AlarmManagement.AlarmFlood real FK (ADR-022) — plain long, matching the shadow reference's primitive key.</summary>
    public long AlarmFloodId { get; }

    public string LinkRole { get; }

    public string? Note { get; }

    public static EventFloodLink Create(
        EventFloodLinkId id, long operationalEventId, long alarmFloodId, string linkRole = "TRIGGER", string? note = null)
    {
        if (string.IsNullOrWhiteSpace(linkRole))
        {
            throw new ArgumentException("EventFloodLink role must not be empty.", nameof(linkRole));
        }

        return new EventFloodLink(id, new OperationalEventId(operationalEventId), alarmFloodId, linkRole, note);
    }
}
