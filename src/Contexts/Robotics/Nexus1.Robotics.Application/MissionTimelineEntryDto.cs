namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 3, verbatim: mission timeline for one mission, ordered by OccurredAtUtc, with the acting robot's code.</summary>
public sealed record MissionTimelineEntryDto(DateTime OccurredAtUtc, string EventCode, string Title, string? RobotCode);
