using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>IncidentAction's defining behavior (ADR-022): records a corrective or preventive action arising from an existing Incident.</summary>
public sealed record RecordIncidentActionCommand(
    long IncidentId, int IncidentActionTypeId, int IncidentActionStatusId, string Title,
    string? Description = null, DateTime? DueAtUtc = null)
    : ICommand<long>;
