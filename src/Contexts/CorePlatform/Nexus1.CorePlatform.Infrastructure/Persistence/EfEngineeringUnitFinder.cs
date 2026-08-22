using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

internal sealed class EfEngineeringUnitFinder(CorePlatformDbContext dbContext) : IEngineeringUnitFinder
{
    public async Task<IReadOnlyList<EngineeringUnit>> GetActiveAsync(CancellationToken cancellationToken) =>
        await dbContext.EngineeringUnits
            .Where(x => x.IsActive)
            .OrderBy(x => x.QuantityType)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Symbol)
            .ToListAsync(cancellationToken);
}
