using Microsoft.EntityFrameworkCore;
using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Instrumentation.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own C.5.8 query 3 (EndedAtUtc IS NULL, quality code IN
/// ('BAD','STALE','UNCERTAIN'), ordered by StartedAtUtc desc), narrowed to one
/// unit via Signal.UnitId to fulfil this operation's own "ForUnit" name — see
/// GetOpenSignalQualityEventsForUnitQuery's doc comment for why.
/// </summary>
internal sealed class EfOpenSignalQualityEventFinder(InstrumentationDbContext dbContext) : IOpenSignalQualityEventFinder
{
    private static readonly string[] OpenQualityCodes = ["BAD", "STALE", "UNCERTAIN"];

    public async Task<IReadOnlyList<OpenSignalQualityEventDto>> GetOpenByUnitCodeAsync(string unitCode, CancellationToken cancellationToken) =>
        await dbContext.SignalQualityEvents
            .Where(e => e.EndedAtUtc == null)
            .Join(dbContext.Signals, e => e.SignalId, s => s.Id, (e, s) => new { e, s })
            .Join(dbContext.Set<ReactorFleetUnitReference>(), x => x.s.UnitId, ru => ru.UnitId, (x, ru) => new { x.e, x.s, ru })
            .Where(x => x.ru.Code == unitCode)
            .Join(dbContext.SignalQualities, x => x.e.SignalQualityId, q => q.Id, (x, q) => new { x.e, x.s, q })
            .Where(x => OpenQualityCodes.Contains(x.q.Code))
            .OrderByDescending(x => x.e.StartedAtUtc)
            .Select(x => new OpenSignalQualityEventDto(x.s.Tag, x.q.Code, x.e.StartedAtUtc, x.e.EndedAtUtc, x.e.ReasonCode))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OpenSignalQualityEventDto>> GetOpenByUnitIdAsync(int unitId, CancellationToken cancellationToken) =>
        await dbContext.SignalQualityEvents
            .Where(e => e.EndedAtUtc == null)
            .Join(dbContext.Signals, e => e.SignalId, s => s.Id, (e, s) => new { e, s })
            .Where(x => x.s.UnitId == unitId)
            .Join(dbContext.SignalQualities, x => x.e.SignalQualityId, q => q.Id, (x, q) => new { x.e, x.s, q })
            .Where(x => OpenQualityCodes.Contains(x.q.Code))
            .OrderByDescending(x => x.e.StartedAtUtc)
            .Select(x => new OpenSignalQualityEventDto(x.s.Tag, x.q.Code, x.e.StartedAtUtc, x.e.EndedAtUtc, x.e.ReasonCode))
            .ToListAsync(cancellationToken);
}
