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

    /// <summary>
    /// Lookup codes (measurement quality) resolved via a small in-memory
    /// dictionary pass after materializing the ordered correlated-subquery
    /// results, not joined inside the ordered subquery — same translation-
    /// safety discipline as RadiationMonitoring's/Robotics' per-unit finders
    /// (joining after OrderByDescending inside a subquery is the exact shape
    /// this project has already found EF Core failing to translate once).
    /// </summary>
    public async Task<IReadOnlyList<UnitSignalReadingDto>> GetSignalReadingsForUnitAsync(int unitId, CancellationToken cancellationToken)
    {
        var signalRows = await dbContext.Signals
            .Where(s => s.IsHistorized && s.UnitId == unitId)
            .Join(dbContext.SignalCategories, s => s.SignalCategoryId, c => c.Id, (s, c) => new { s, c })
            .OrderBy(x => x.s.Tag)
            .Select(x => new
            {
                x.s.Tag,
                x.s.Name,
                CategoryCode = x.c.Code,
                LatestValue = dbContext.Measurements
                    .Where(m => m.SignalId == x.s.Id)
                    .OrderByDescending(m => m.TimestampUtc)
                    .Select(m => m.NumericValue)
                    .FirstOrDefault(),
                LatestTimestampUtc = dbContext.Measurements
                    .Where(m => m.SignalId == x.s.Id)
                    .OrderByDescending(m => m.TimestampUtc)
                    .Select(m => (DateTime?)m.TimestampUtc)
                    .FirstOrDefault(),
                LatestQualityId = dbContext.Measurements
                    .Where(m => m.SignalId == x.s.Id)
                    .OrderByDescending(m => m.TimestampUtc)
                    .Select(m => (int?)m.SignalQualityId.Value)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var qualityCodesById = await dbContext.SignalQualities
            .ToDictionaryAsync(q => q.Id.Value, q => q.Code, cancellationToken);

        return signalRows
            .Select(x => new UnitSignalReadingDto(
                x.Tag,
                x.Name,
                x.CategoryCode,
                x.LatestValue,
                x.LatestQualityId is int qId && qualityCodesById.TryGetValue(qId, out var qCode) ? qCode : null,
                x.LatestTimestampUtc))
            .ToList();
    }
}
