using Microsoft.EntityFrameworkCore;
using Nexus1.EmergencyPreparedness.Application;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own verification query 4, adapted: EmergencyResource
/// JOIN ResourceType, each resource's latest ResourceReadinessCheck's
/// ReadinessStatus code via the correlated-subquery "latest per parent row"
/// pattern (`OrderByDescending().Select().FirstOrDefault()`, nested so the
/// ReadinessStatus code lookup is itself a scalar subquery rather than a
/// LEFT JOIN against a correlated-subquery key — a genuine LEFT JOIN whose
/// outer key selector is itself a correlated subquery does NOT translate
/// here, caught by the real component test against LocalDB
/// ("InvalidOperationException: ... could not be translated"), not by the
/// rule/pipeline-layer tests alone. GROUP BY SiteId/ResourceType.Code/
/// ReadinessStatus.Code happens client-side-safe as a second LINQ query
/// over the first (still fully server-translated: two nested subqueries,
/// no client evaluation).
///
/// Deviation from the atlas's own literal query text: the atlas projects
/// Organization.Site's own code directly, which assumes single-database
/// access. In this codebase Organization.Site is passport-only —
/// OrganizationDb is a different physical database than AlarmManagementDb
/// (ADR-025) — so SiteId is projected instead of a site code, matching
/// EfSiteActivePlansFinder's own adaptation (see
/// ResourceReadinessDashboardDto's own doc comment).
/// </summary>
internal sealed class EfResourceReadinessDashboardFinder(EmergencyPreparednessDbContext dbContext) : IResourceReadinessDashboardFinder
{
    public async Task<IReadOnlyList<ResourceReadinessDashboardDto>> GetResourceReadinessDashboardAsync(CancellationToken cancellationToken)
    {
        var perResource =
            from res in dbContext.EmergencyResources
            join t in dbContext.ResourceTypes on res.ResourceTypeId equals t.Id
            select new
            {
                res.SiteId,
                ResourceTypeCode = t.Code,
                ReadinessStatusCode = dbContext.ResourceReadinessChecks
                    .Where(c => c.EmergencyResourceId == res.Id)
                    .OrderByDescending(c => c.CheckedAtUtc)
                    .Select(c => dbContext.ReadinessStatuses
                        .Where(rs => rs.Id == c.ReadinessStatusId)
                        .Select(rs => rs.Code)
                        .FirstOrDefault())
                    .FirstOrDefault(),
            };

        var grouped =
            from x in perResource
            group x by new { x.SiteId, x.ResourceTypeCode, x.ReadinessStatusCode } into g
            select new ResourceReadinessDashboardDto(g.Key.SiteId, g.Key.ResourceTypeCode, g.Key.ReadinessStatusCode, g.Count());

        return await grouped.ToListAsync(cancellationToken);
    }
}
