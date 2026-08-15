using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Infrastructure.Persistence;

/// <summary>
/// Not the generic EfRepository&lt;TRoot,TId&gt; pattern ReactorFleet/
/// AlarmManagement use — RootCauseAnalysis has child collections
/// (Hypotheses, each with its own Evidence), and a bare FindAsync would not
/// eager-load them, silently handing callers an aggregate that looks empty.
/// </summary>
internal sealed class RootCauseAnalysisRepository(RootCauseDbContext dbContext)
    : IRepository<RootCauseAnalysis, RootCauseAnalysisId>
{
    public async Task<RootCauseAnalysis?> GetByIdAsync(RootCauseAnalysisId id, CancellationToken cancellationToken) =>
        await dbContext.RootCauseAnalyses
            .Include(a => a.Hypotheses)
            .ThenInclude(h => h.Evidence)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(RootCauseAnalysis entity, CancellationToken cancellationToken) =>
        await dbContext.RootCauseAnalyses.AddAsync(entity, cancellationToken);
}
