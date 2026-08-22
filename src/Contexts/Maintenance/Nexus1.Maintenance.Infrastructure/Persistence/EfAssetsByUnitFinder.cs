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
}
