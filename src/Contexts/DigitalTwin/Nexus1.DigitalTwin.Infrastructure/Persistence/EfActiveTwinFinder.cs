using Microsoft.EntityFrameworkCore;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.6.8 query 1 exactly: every active (IsDeleted = 0) twin joined to its unit, model type, status and fidelity.</summary>
internal sealed class EfActiveTwinFinder(DigitalTwinDbContext dbContext) : IActiveTwinFinder
{
    public async Task<IReadOnlyList<ActiveTwinDto>> GetActiveTwinsAsync(CancellationToken cancellationToken) =>
        await dbContext.TwinModels
            .Where(tm => !tm.IsDeleted)
            .Join(dbContext.Set<ReactorFleetUnitReference>(), tm => tm.UnitId, u => u.UnitId, (tm, u) => new { tm, u })
            .Join(dbContext.TwinModelTypes, x => x.tm.TwinModelTypeId, tmt => tmt.Id, (x, tmt) => new { x.tm, x.u, tmt })
            .Join(dbContext.TwinModelStatuses, x => x.tm.TwinModelStatusId, tms => tms.Id, (x, tms) => new { x.tm, x.u, x.tmt, tms })
            .Join(dbContext.TwinFidelityLevels, x => x.tm.TwinFidelityLevelId, tfl => tfl.Id, (x, tfl) => new { x.tm, x.u, x.tmt, x.tms, tfl })
            .Select(x => new ActiveTwinDto(x.u.Code, x.tm.Code, x.tmt.Code, x.tms.Code, x.tfl.Code))
            .ToListAsync(cancellationToken);
}
