using Microsoft.EntityFrameworkCore;
using Nexus1.Maintenance.Application;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.9.5.2 query 5 exactly: active (IsActive = 1) degradation records with mechanism, severity and trend point count, ordered by DetectedAtUtc desc.</summary>
internal sealed class EfActiveDegradationCasesFinder(MaintenanceDbContext dbContext) : IActiveDegradationCasesFinder
{
    public async Task<IReadOnlyList<ActiveDegradationCaseDto>> GetActiveDegradationCasesAsync(CancellationToken cancellationToken)
    {
        var query =
            from d in dbContext.DegradationRecords
            where d.IsActive
            join a in dbContext.Assets on d.AssetId equals a.Id
            join m in dbContext.DegradationMechanisms on d.DegradationMechanismId equals m.Id
            join sev in dbContext.FindingSeverities on d.FindingSeverityId equals sev.Id
            orderby d.DetectedAtUtc descending
            select new ActiveDegradationCaseDto(
                a.AssetCode, m.Code, sev.Code, d.DetectedAtUtc,
                dbContext.DegradationTrendPoints.Count(tp => tp.DegradationRecordId == d.Id));

        return await query.ToListAsync(cancellationToken);
    }
}
