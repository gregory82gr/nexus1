using Microsoft.EntityFrameworkCore;
using Nexus1.Instrumentation.Application;

namespace Nexus1.Instrumentation.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.5.8 query 4 exactly: tag -> current SignalMapping (EffectiveToUtc IS NULL) -> AcquisitionPoint -> AcquisitionConnection -> DataAcquisitionNode.</summary>
internal sealed class EfAcquisitionPathFinder(InstrumentationDbContext dbContext) : IAcquisitionPathFinder
{
    public async Task<IReadOnlyList<AcquisitionPathDto>> GetByTagAsync(string tag, CancellationToken cancellationToken) =>
        await dbContext.Signals
            .Where(s => s.Tag == tag)
            .Join(dbContext.SignalMappings.Where(sm => sm.EffectiveToUtc == null), s => s.Id, sm => sm.SignalId, (s, sm) => new { s, sm })
            .Join(dbContext.AcquisitionPoints, x => x.sm.AcquisitionPointId, p => p.Id, (x, p) => new { x.s, p })
            .Join(dbContext.AcquisitionConnections, x => x.p.AcquisitionConnectionId, c => c.Id, (x, c) => new { x.s, x.p, c })
            .Join(dbContext.DataAcquisitionNodes, x => x.c.DataAcquisitionNodeId, n => n.Id, (x, n) => new { x.s, x.p, x.c, n })
            .Select(x => new AcquisitionPathDto(x.s.Tag, x.n.Code, x.c.Protocol, x.c.Endpoint, x.p.RawAddress))
            .ToListAsync(cancellationToken);
}
