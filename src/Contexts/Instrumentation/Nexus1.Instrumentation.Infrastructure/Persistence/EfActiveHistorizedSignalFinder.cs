using Microsoft.EntityFrameworkCore;
using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Instrumentation.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.5.8 query 1 exactly: active historized signals for one unit, ordered by Tag.</summary>
internal sealed class EfActiveHistorizedSignalFinder(InstrumentationDbContext dbContext) : IActiveHistorizedSignalFinder
{
    public async Task<IReadOnlyList<ActiveHistorizedSignalDto>> GetByUnitCodeAsync(string unitCode, CancellationToken cancellationToken) =>
        await dbContext.Signals
            .Where(s => s.IsHistorized)
            .Join(dbContext.SignalCategories, s => s.SignalCategoryId, c => c.Id, (s, c) => new { s, c })
            .Join(dbContext.HistorianRetentionClasses, x => x.s.HistorianRetentionClassId, r => r.Id, (x, r) => new { x.s, x.c, r })
            .Join(dbContext.Set<CorePlatformEngineeringUnitReference>(), x => x.s.EngineeringUnitId, u => u.EngineeringUnitId, (x, u) => new { x.s, x.c, x.r, u })
            .Join(dbContext.Set<ReactorFleetUnitReference>(), x => x.s.UnitId, ru => ru.UnitId, (x, ru) => new { x.s, x.c, x.r, x.u, ru })
            .Where(x => x.ru.Code == unitCode)
            .OrderBy(x => x.s.Tag)
            .Select(x => new ActiveHistorizedSignalDto(x.s.Tag, x.s.Name, x.c.Code, x.u.Symbol, x.r.Code))
            .ToListAsync(cancellationToken);
}
