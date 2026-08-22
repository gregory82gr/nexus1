using Microsoft.EntityFrameworkCore;
using Nexus1.ReinforcementLearning.Application;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

/// <summary>Matches the atlas's own verification query 2, verbatim: QTable WHERE IsFinal = 1 JOIN QTableEntry, GROUP BY QTable.Code, COUNT.</summary>
internal sealed class EfFinalQTableEntryCountFinder(ReinforcementLearningDbContext dbContext) : IFinalQTableEntryCountFinder
{
    public async Task<IReadOnlyList<FinalQTableEntryCountDto>> GetFinalQTableEntryCountsAsync(CancellationToken cancellationToken)
    {
        var query =
            from q in dbContext.QTables
            where q.IsFinal
            select new FinalQTableEntryCountDto(q.Code, dbContext.QTableEntries.Count(e => e.QTableId == q.Id));

        return await query.ToListAsync(cancellationToken);
    }
}
