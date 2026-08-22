using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>Atlas C.8.5.2 query 3, verbatim: incident actions where status NOT IN (COMPLETED, VERIFIED, CANCELLED), with incident number, ordered by DueAtUtc.</summary>
public sealed record GetOpenIncidentActionsQuery : IQuery<IReadOnlyList<OpenIncidentActionDto>>;
