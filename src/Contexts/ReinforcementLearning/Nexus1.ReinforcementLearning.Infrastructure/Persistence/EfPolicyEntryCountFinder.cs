using Microsoft.EntityFrameworkCore;
using Nexus1.ReinforcementLearning.Application;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

/// <summary>Matches the atlas's own verification query 1, verbatim: Policy JOIN PolicyEntry, GROUP BY Policy.Code, COUNT.</summary>
internal sealed class EfPolicyEntryCountFinder(ReinforcementLearningDbContext dbContext) : IPolicyEntryCountFinder
{
    public async Task<IReadOnlyList<PolicyEntryCountDto>> GetPolicyEntryCountsAsync(CancellationToken cancellationToken)
    {
        var query =
            from p in dbContext.Policies
            select new PolicyEntryCountDto(p.Code, dbContext.PolicyEntries.Count(e => e.PolicyId == p.Id));

        return await query.ToListAsync(cancellationToken);
    }
}
