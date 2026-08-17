using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>Atlas C.8.5.2 query 1, verbatim: an event by EventCode, with status/severity codes, plus any linked AlarmEventIds and AlarmFloodIds via LEFT JOINs.</summary>
public sealed record GetEventWithAlarmsAndFloodQuery(string EventCode) : IQuery<EventWithAlarmsAndFloodDto?>;
