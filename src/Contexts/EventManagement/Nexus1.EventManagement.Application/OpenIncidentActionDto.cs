namespace Nexus1.EventManagement.Application;

/// <summary>Atlas C.8.5.2 query 3 projection, verbatim: IncidentNumber, Title, ActionStatus code, DueAtUtc.</summary>
public sealed record OpenIncidentActionDto(string IncidentNumber, string Title, string ActionStatus, DateTime? DueAtUtc);
