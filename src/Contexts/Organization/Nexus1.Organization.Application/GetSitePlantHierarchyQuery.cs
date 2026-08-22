using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

/// <summary>Atlas C.3.8 query 1, verbatim.</summary>
public sealed record GetSitePlantHierarchyQuery(string SiteCode) : IQuery<SitePlantHierarchyDto>;
