using Microsoft.EntityFrameworkCore;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.9.5.2 query 1 as closely as this database can build it: every non-deleted asset joined to its unit, category and status, ordered by unit code then asset code — see AssetByUnitDto for why EquipmentId is the raw passport, not a joined EquipmentCode.</summary>
internal sealed class EfAssetsByUnitFinder(MaintenanceDbContext dbContext) : IAssetsByUnitFinder
{
    public async Task<IReadOnlyList<AssetByUnitDto>> GetAssetsByUnitAsync(CancellationToken cancellationToken)
    {
        var query =
            from a in dbContext.Assets
            where !a.IsDeleted
            join u in dbContext.Set<ReactorFleetUnitReference>() on a.UnitId equals u.UnitId
            join ac in dbContext.AssetCategories on a.AssetCategoryId equals ac.Id
            join ast in dbContext.AssetStatuses on a.AssetStatusId equals ast.Id
            orderby u.Code, a.AssetCode
            select new AssetByUnitDto(u.Code, a.AssetCode, a.Name, ac.Code, ast.Code, a.EquipmentId);

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Latest condition resolved via the same correlated-subquery pattern
    /// EfLatestConditionPerAssetFinder already uses (OrderByDescending +
    /// FirstOrDefault); ConditionGrade codes resolved via a separate
    /// in-memory dictionary pass afterward, same translation-safety
    /// discipline already established for several other contexts' per-unit
    /// finders (joining inside an ordered subquery is the shape this
    /// project has already found EF Core failing to translate once).
    /// </summary>
    public async Task<IReadOnlyList<UnitAssetConditionDto>> GetAssetConditionsForUnitAsync(int unitId, CancellationToken cancellationToken)
    {
        var query =
            from a in dbContext.Assets
            where !a.IsDeleted && a.UnitId == unitId
            join ac in dbContext.AssetCategories on a.AssetCategoryId equals ac.Id
            join ast in dbContext.AssetStatuses on a.AssetStatusId equals ast.Id
            orderby a.AssetCode
            select new
            {
                a.AssetCode,
                a.Name,
                Category = ac.Code,
                Status = ast.Code,
                a.IsSafetyRelated,
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
            .Select(r => new UnitAssetConditionDto(
                r.AssetCode, r.Name, r.Category, r.Status, r.IsSafetyRelated,
                r.Latest?.AssessedAtUtc,
                r.Latest is not null && conditionGrades.TryGetValue(r.Latest.ConditionGradeId, out var code) ? code : null,
                r.Latest?.HealthScorePercent, r.Latest?.RemainingUsefulLifeDays))
            .ToList();
    }
}
