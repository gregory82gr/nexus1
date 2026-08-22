namespace Nexus1.ReinforcementLearning.Application;

public interface IClampedRecommendationsFinder
{
    Task<IReadOnlyList<ClampedRecommendationDto>> GetClampedRecommendationsAsync(CancellationToken cancellationToken);
}
