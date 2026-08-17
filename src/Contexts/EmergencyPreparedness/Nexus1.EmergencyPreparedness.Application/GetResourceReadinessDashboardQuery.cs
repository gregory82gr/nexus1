using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>Atlas verification query 4, verbatim: EmergencyResource JOIN ResourceType, each resource's latest ResourceReadinessCheck.ReadinessStatusId (correlated subquery), LEFT JOIN ReadinessStatus, GROUP BY SiteId/ResourceType.Code/ReadinessStatus.Code.</summary>
public sealed record GetResourceReadinessDashboardQuery : IQuery<IReadOnlyList<ResourceReadinessDashboardDto>>;
