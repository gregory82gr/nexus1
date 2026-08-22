using Microsoft.EntityFrameworkCore;
using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

/// <summary>Matches the atlas's own verification query 3, verbatim: PolicyEntry JOIN StateDefinition/ActionDefinition, ORDER BY StateIndex.</summary>
internal sealed class EfPolicyGridFinder(ReinforcementLearningDbContext dbContext) : IPolicyGridFinder
{
    public async Task<IReadOnlyList<PolicyGridEntryDto>> GetPolicyGridAsync(int policyId, CancellationToken cancellationToken)
    {
        var id = new PolicyId(policyId);

        var query =
            from pe in dbContext.PolicyEntries
            where pe.PolicyId == id
            join sd in dbContext.StateDefinitions on pe.StateDefinitionId equals sd.Id
            join ad in dbContext.ActionDefinitions on pe.BestActionDefinitionId equals ad.Id
            orderby sd.StateIndex
            select new PolicyGridEntryDto(sd.StateIndex, sd.Code, ad.Code, pe.BestQValue, pe.ActionMargin);

        return await query.ToListAsync(cancellationToken);
    }
}
