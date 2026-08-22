using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class AdvisoryRecommendationTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_defaults()
    {
        var recommendation = AdvisoryRecommendation.Create(
            new AdvisoryRecommendationId(1), new AdvisorySessionId(1), new RecommendationStatusId(1),
            new StateDefinitionId(1), new ActionDefinitionId(1), NowUtc);

        Assert.False(recommendation.WasClamped);
        Assert.Null(recommendation.ClampedActionDefinitionId);
        Assert.Null(recommendation.ClampReason);
        Assert.Equal(NowUtc, recommendation.RequestedAtUtc);
    }

    [Fact]
    public void Create_with_clamp_sets_both_recommended_and_clamped_action_ids_side_by_side()
    {
        var recommendation = AdvisoryRecommendation.Create(
            new AdvisoryRecommendationId(1), new AdvisorySessionId(1), new RecommendationStatusId(1),
            new StateDefinitionId(1), new ActionDefinitionId(3), NowUtc,
            clampedActionDefinitionId: new ActionDefinitionId(1), wasClamped: true,
            clampReason: "Clamped to validated band.");

        Assert.Equal(new ActionDefinitionId(3), recommendation.RecommendedActionDefinitionId);
        Assert.Equal(new ActionDefinitionId(1), recommendation.ClampedActionDefinitionId);
        Assert.True(recommendation.WasClamped);
        Assert.Equal("Clamped to validated band.", recommendation.ClampReason);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Create_with_out_of_range_confidence_score_throws(double confidenceScore)
    {
        Assert.Throws<ArgumentException>(() => AdvisoryRecommendation.Create(
            new AdvisoryRecommendationId(1), new AdvisorySessionId(1), new RecommendationStatusId(1),
            new StateDefinitionId(1), new ActionDefinitionId(1), NowUtc, confidenceScore: (decimal)confidenceScore));
    }

    [Fact]
    public void Create_with_in_range_confidence_score_succeeds()
    {
        var recommendation = AdvisoryRecommendation.Create(
            new AdvisoryRecommendationId(1), new AdvisorySessionId(1), new RecommendationStatusId(1),
            new StateDefinitionId(1), new ActionDefinitionId(1), NowUtc, confidenceScore: 0.87m);

        Assert.Equal(0.87m, recommendation.ConfidenceScore);
    }
}
