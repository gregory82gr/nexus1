using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>
/// A chronological narrative entry for an OperationalEvent (atlas C.8.1,
/// query 2's subject): "gives the event a replayable chronology."
/// OperationalEventId and EventTimelineEntryTypeId are real internal FKs.
/// EnteredByUserId is passport-only (Security.ApplicationUser, a different
/// physical database). No audit columns in the real DDL for this table —
/// leaner than OperationalEvent/Incident/IncidentAction (ADR-022).
/// </summary>
public sealed class EventTimelineEntry : Entity<EventTimelineEntryId>, IAggregateRoot
{
    private EventTimelineEntry(
        EventTimelineEntryId id, OperationalEventId operationalEventId, EventTimelineEntryTypeId eventTimelineEntryTypeId,
        DateTime entryAtUtc, string title, string? body, int? enteredByUserId, string? sourceReference)
        : base(id)
    {
        OperationalEventId = operationalEventId;
        EventTimelineEntryTypeId = eventTimelineEntryTypeId;
        EntryAtUtc = entryAtUtc;
        Title = title;
        Body = body;
        EnteredByUserId = enteredByUserId;
        SourceReference = sourceReference;
    }

    /// <summary>Real internal FK to OperationalEvent — typed to match OperationalEvent's own strongly-typed Id.</summary>
    public OperationalEventId OperationalEventId { get; }

    public EventTimelineEntryTypeId EventTimelineEntryTypeId { get; }

    public DateTime EntryAtUtc { get; }

    public string Title { get; }

    public string? Body { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-022).</summary>
    public int? EnteredByUserId { get; }

    public string? SourceReference { get; }

    public static EventTimelineEntry Create(
        EventTimelineEntryId id, long operationalEventId, EventTimelineEntryTypeId eventTimelineEntryTypeId,
        DateTime entryAtUtc, string title, string? body = null, int? enteredByUserId = null, string? sourceReference = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("EventTimelineEntry title must not be empty.", nameof(title));
        }

        return new EventTimelineEntry(id, new OperationalEventId(operationalEventId), eventTimelineEntryTypeId, entryAtUtc, title, body, enteredByUserId, sourceReference);
    }
}
