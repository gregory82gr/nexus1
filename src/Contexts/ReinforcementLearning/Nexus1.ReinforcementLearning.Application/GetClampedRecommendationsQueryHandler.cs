using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class GetClampedRecommendationsQueryHandler(IClampedRecommendationsFinder finder)
    : IQueryHandler<GetClampedRecommendationsQuery, IReadOnlyList<ClampedRecommendationDto>>
{
    public async Task<Result<IReadOnlyList<ClampedRecommendationDto>>> Handle(GetClampedRecommendationsQuery query, CancellationToken cancellationToken)
    {
        var recommendations = await finder.GetClampedRecommendationsAsync(cancellationToken);
        return Result<IReadOnlyList<ClampedRecommendationDto>>.Success(recommendations);
    }
}
