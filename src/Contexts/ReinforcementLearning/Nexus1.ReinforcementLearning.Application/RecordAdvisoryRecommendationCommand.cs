using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

/// <summary>
/// AdvisoryRecommendation's defining behavior (ADR-026): completes the
/// pipeline by recording a clamped recommendation against an
/// AdvisorySession, giving query 4 real data to exercise.
/// </summary>
public sealed record RecordAdvisoryRecommendationCommand(
    long AdvisorySessionId, int RecommendationStatusId, int StateDefinitionId, int RecommendedActionDefinitionId,
    DateTime RequestedAtUtc, int? ClampedActionDefinitionId = null, decimal? ObservedPowerPercent = null,
    decimal? TargetPowerPercent = null, decimal? ConfidenceScore = null, bool WasClamped = false,
    string? ClampReason = null, DateTime? ExpiresAtUtc = null)
    : ICommand<long>;
