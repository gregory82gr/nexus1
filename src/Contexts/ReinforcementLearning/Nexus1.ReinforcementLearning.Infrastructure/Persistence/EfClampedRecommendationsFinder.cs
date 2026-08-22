using Microsoft.EntityFrameworkCore;
using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own verification query 4, verbatim: AdvisoryRecommendation
/// WHERE WasClamped = 1, JOIN StateDefinition, JOIN ActionDefinition for the
/// recommended action, LEFT JOIN ActionDefinition again for the clamped
/// action (two separate joins to the same table, aliased distinctly),
/// ORDER BY RequestedAtUtc DESC.
/// </summary>
internal sealed class EfClampedRecommendationsFinder(ReinforcementLearningDbContext dbContext) : IClampedRecommendationsFinder
{
    public async Task<IReadOnlyList<ClampedRecommendationDto>> GetClampedRecommendationsAsync(CancellationToken cancellationToken)
    {
        var query =
            from ar in dbContext.AdvisoryRecommendations
            where ar.WasClamped
            join sd in dbContext.StateDefinitions on ar.StateDefinitionId equals sd.Id
            join recommended in dbContext.ActionDefinitions on ar.RecommendedActionDefinitionId equals recommended.Id
            join clamped in dbContext.ActionDefinitions on ar.ClampedActionDefinitionId equals (ActionDefinitionId?)clamped.Id into clampedGroup
            from clamped in clampedGroup.DefaultIfEmpty()
            orderby ar.RequestedAtUtc descending
            select new ClampedRecommendationDto(
                ar.Id.Value, ar.RequestedAtUtc, sd.Code, recommended.Code,
                clamped == null ? null : clamped.Code, ar.ClampReason);

        return await query.ToListAsync(cancellationToken);
    }
}
