using Microsoft.EntityFrameworkCore;
using Nexus1.EmergencyPreparedness.Application;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own verification query 1, adapted: EmergencyPlan
/// WHERE SiteId = @siteId AND IsDeleted = 0, JOIN PlanStatus, with a
/// revision-row count from EmergencyPlanRevision (correlated COUNT
/// subquery rather than a GroupBy+Join, which this codebase's own prior
/// sectors found does not always translate cleanly to SQL).
///
/// Deviation from the atlas's own literal query text: the atlas projects
/// Organization.Site's own code directly, which assumes single-database
/// access. In this codebase Organization.Site is passport-only —
/// OrganizationDb is a different physical database than AlarmManagementDb
/// (ADR-025) — so there is no local Site table to join against. This
/// finder accepts SiteId as a plain int filter and does not attempt to
/// project a site code, matching how RadiationMonitoring's own
/// EfOpenDoseAlertsFinder handled the same class of cross-database gap
/// (see ActiveEmergencyPlanDto's own doc comment).
/// </summary>
internal sealed class EfSiteActivePlansFinder(EmergencyPreparednessDbContext dbContext) : ISiteActivePlansFinder
{
    public async Task<IReadOnlyList<ActiveEmergencyPlanDto>> GetActivePlansAsync(int siteId, CancellationToken cancellationToken)
    {
        var query =
            from p in dbContext.EmergencyPlans
            where p.SiteId == siteId && !EF.Property<bool>(p, "IsDeleted")
            join s in dbContext.PlanStatuses on p.PlanStatusId equals s.Id
            select new ActiveEmergencyPlanDto(
                p.Code, s.Code, p.CurrentRevisionNumber,
                dbContext.EmergencyPlanRevisions.Count(r => r.EmergencyPlanId == p.Id));

        return await query.ToListAsync(cancellationToken);
    }
}
