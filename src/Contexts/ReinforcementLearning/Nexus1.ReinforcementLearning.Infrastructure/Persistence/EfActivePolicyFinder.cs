using Microsoft.EntityFrameworkCore;
using Nexus1.ReinforcementLearning.Application;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

/// <summary>
/// "Active" here means: the most recently extracted Policy whose source
/// QTable is IsFinal. Not an atlas-named query — this finder's own
/// definition, since no IsCurrent/IsActive concept exists on Policy itself
/// (see IActivePolicyFinder's own doc comment for the full reasoning).
/// </summary>
internal sealed class EfActivePolicyFinder(ReinforcementLearningDbContext dbContext) : IActivePolicyFinder
{
    public async Task<int?> GetActivePolicyIdAsync(CancellationToken cancellationToken)
    {
        var query =
            from p in dbContext.Policies
            join qt in dbContext.QTables on p.QTableId equals qt.Id
            where qt.IsFinal
            orderby p.ExtractedAtUtc descending
            select (int?)p.Id.Value;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
