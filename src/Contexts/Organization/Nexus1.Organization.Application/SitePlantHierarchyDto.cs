namespace Nexus1.Organization.Application;

public sealed record SitePlantHierarchyDto(int SiteId, string SiteCode, string SiteName, IReadOnlyList<PlantSummaryDto> Plants);
