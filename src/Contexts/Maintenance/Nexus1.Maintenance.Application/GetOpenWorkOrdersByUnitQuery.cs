using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>Atlas C.9.5.2 query 2, verbatim: open work orders (IsDeleted = 0, status not CLOSED/CANCELLED) with unit, status and priority, ordered by DueAtUtc.</summary>
public sealed record GetOpenWorkOrdersByUnitQuery : IQuery<IReadOnlyList<OpenWorkOrderDto>>;
