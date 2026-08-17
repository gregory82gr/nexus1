using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>Atlas C.8.5.2 query 2, verbatim: timeline entries for an event, ordered by EntryAtUtc, with entry-type code.</summary>
public sealed record GetEventTimelineQuery(long OperationalEventId) : IQuery<IReadOnlyList<EventTimelineEntryDto>>;
