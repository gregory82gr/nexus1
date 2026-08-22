using Microsoft.EntityFrameworkCore;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.9.5.2 query 4: latest AssetCondition per Asset via a correlated subquery (OrderByDescending + FirstOrDefault, EF Core's LINQ equivalent of the atlas's OUTER APPLY TOP 1), with condition grade, ordered by AssetCode.</summary>
internal sealed class EfLatestConditionPerAssetFinder(MaintenanceDbContext dbContext) : ILatestConditionPerAssetFinder
{
    public async Task<IReadOnlyList<LatestConditionDto>> GetLatestConditionPerAssetAsync(CancellationToken cancellationToken)
    {
        var query =
            from a in dbContext.Assets
            orderby a.AssetCode
            select new
            {
                a.AssetCode,
                a.Name,
                Latest = dbContext.AssetConditions
                    .Where(c => c.AssetId == a.Id)
                    .OrderByDescending(c => c.AssessedAtUtc)
                    .FirstOrDefault(),
            };

        var rows = await query.ToListAsync(cancellationToken);

        var conditionGradeIds = rows.Where(r => r.Latest is not null).Select(r => r.Latest!.ConditionGradeId).Distinct().ToList();
        var conditionGrades = await dbContext.ConditionGrades
            .Where(g => conditionGradeIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Code, cancellationToken);

        return rows
            .Select(r => new LatestConditionDto(
                r.AssetCode, r.Name, r.Latest?.AssessedAtUtc,
                r.Latest is not null && conditionGrades.TryGetValue(r.Latest.ConditionGradeId, out var code) ? code : null,
                r.Latest?.HealthScorePercent, r.Latest?.RemainingUsefulLifeDays))
            .ToList();
    }
}
