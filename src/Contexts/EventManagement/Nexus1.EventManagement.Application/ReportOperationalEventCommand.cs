using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>OperationalEvent's defining behavior (ADR-022): reports a new structured operational occurrence against a unit.</summary>
public sealed record ReportOperationalEventCommand(
    int UnitId, int EventTypeId, int EventStatusId, int EventSeverityId, int EventSourceTypeId, string EventCode,
    string Title, DateTime DetectedAtUtc, DateTime ReportedAtUtc, int? PlantId = null, string? Summary = null,
    DateTime? ClosedAtUtc = null, int? ReportedByUserId = null, int? OwnerUserId = null, int? OwningPersonId = null,
    string? SourceReference = null, bool IsDrill = false)
    : ICommand<long>;
