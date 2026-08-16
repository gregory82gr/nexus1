namespace Nexus1.Organization.Application;

/// <summary>Matches the atlas's own C.3.8 query 1: site code -> plant code/name list for that site.</summary>
public interface ISitePlantHierarchyFinder
{
    Task<SitePlantHierarchyDto?> GetBySiteCodeAsync(string siteCode, CancellationToken cancellationToken);
}
