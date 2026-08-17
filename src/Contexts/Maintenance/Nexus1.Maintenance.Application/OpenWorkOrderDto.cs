namespace Nexus1.Maintenance.Application;

/// <summary>Atlas C.9.5.2 query 2 projection, verbatim: UnitCode, WorkOrderCode, Title, Status, Priority, DueAtUtc.</summary>
public sealed record OpenWorkOrderDto(string UnitCode, string WorkOrderCode, string Title, string Status, string Priority, DateTime? DueAtUtc);
