namespace Nexus1.EventManagement.Application;

/// <summary>Atlas C.8.5.2 query 1 projection, verbatim: EventCode, Title, EventStatus code, EventSeverity code, plus every linked AlarmEventId and AlarmFloodId (the query's LEFT JOINs can fan out to zero or more of each).</summary>
public sealed record EventWithAlarmsAndFloodDto(
    string EventCode, string Title, string EventStatus, string EventSeverity,
    IReadOnlyList<long> AlarmEventIds, IReadOnlyList<long> AlarmFloodIds);
