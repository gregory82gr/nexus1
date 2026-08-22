using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>Atlas verification query 1, verbatim: one site's active (IsDeleted = 0) EmergencyPlan rows, joined to PlanStatus, with a revision-row count.</summary>
public sealed record GetSiteActivePlansQuery(int SiteId) : IQuery<IReadOnlyList<ActiveEmergencyPlanDto>>;
