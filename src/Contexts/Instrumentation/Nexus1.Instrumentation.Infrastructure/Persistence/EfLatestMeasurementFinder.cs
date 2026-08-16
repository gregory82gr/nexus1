using Microsoft.EntityFrameworkCore;
using Nexus1.Instrumentation.Application;

namespace Nexus1.Instrumentation.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.5.8 query 2 exactly: TOP (count) most recent values for a tag, newest first.</summary>
internal sealed class EfLatestMeasurementFinder(InstrumentationDbContext dbContext) : ILatestMeasurementFinder
{
    public async Task<IReadOnlyList<LatestMeasurementDto>> GetLatestByTagAsync(string tag, int count, CancellationToken cancellationToken) =>
        await dbContext.Measurements
            .Join(dbContext.Signals, m => m.SignalId, s => s.Id, (m, s) => new { m, s })
            .Where(x => x.s.Tag == tag)
            .Join(dbContext.SignalQualities, x => x.m.SignalQualityId, q => q.Id, (x, q) => new { x.m, x.s, q })
            .Join(dbContext.MeasurementSources, x => x.m.MeasurementSourceId, src => src.Id, (x, src) => new { x.m, x.q, src })
            .OrderByDescending(x => x.m.TimestampUtc)
            .Take(count)
            .Select(x => new LatestMeasurementDto(x.m.TimestampUtc, x.m.NumericValue, x.q.Code, x.src.Code))
            .ToListAsync(cancellationToken);
}
