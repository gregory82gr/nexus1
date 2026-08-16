using Microsoft.EntityFrameworkCore;
using Nexus1.Organization.Application;

namespace Nexus1.Organization.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.3.8 query 1 exactly: site code -> plant code/name list for that site.</summary>
internal sealed class EfSitePlantHierarchyFinder(OrganizationDbContext dbContext) : ISitePlantHierarchyFinder
{
    public async Task<SitePlantHierarchyDto?> GetBySiteCodeAsync(string siteCode, CancellationToken cancellationToken)
    {
        var site = await dbContext.Sites
            .Where(x => x.Code == siteCode)
            .Select(x => new { x.Id, x.Code, x.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (site is null)
        {
            return null;
        }

        var plants = await dbContext.Plants
            .Where(p => p.SiteId == site.Id)
            .OrderBy(p => p.Code)
            .Select(p => new PlantSummaryDto(p.Id.Value, p.Code, p.Name))
            .ToListAsync(cancellationToken);

        return new SitePlantHierarchyDto(site.Id.Value, site.Code, site.Name, plants);
    }
}
