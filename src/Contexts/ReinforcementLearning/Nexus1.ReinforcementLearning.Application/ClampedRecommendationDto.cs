namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Atlas verification query 4, verbatim: "review clamped advisory recommendations."</summary>
public sealed record ClampedRecommendationDto(
    long AdvisoryRecommendationId, DateTime RequestedAtUtc, string StateCode, string RecommendedActionCode,
    string? ClampedActionCode, string? ClampReason);
