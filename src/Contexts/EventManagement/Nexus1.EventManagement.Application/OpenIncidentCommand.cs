using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>Incident's defining behavior (ADR-022): opens a new incident from an existing OperationalEvent. Fails with a clear conflict if that event already has an open incident (OperationalEventId is unique).</summary>
public sealed record OpenIncidentCommand(
    long OperationalEventId, int IncidentTypeId, int IncidentStatusId, string IncidentNumber, DateTime OpenedAtUtc,
    string? InvestigationSummary = null, int? LeadInvestigatorUserId = null)
    : ICommand<long>;
