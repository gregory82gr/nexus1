using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// One clamped, human-facing advisory record — carries both
/// RecommendedActionDefinitionId (the raw policy pick) and
/// ClampedActionDefinitionId (after the safety clamp) side by side, so
/// "what the table said" vs. "what was actually offered" survives in the
/// data the way Chapter 10 insists it must survive in the UI (atlas
/// C.11.2, C.11.5.2 query 4's own subject, ADR-026). Nothing here models
/// actuation — no execution timestamp, no command target, no
/// authorization token; it advises, it does not act. RequestedAtUtc has a
/// SQL DEFAULT (SYSUTCDATETIME()) but is still a required constructor
/// param, same pattern as QTable.SnapshotAtUtc/AdvisorySession.StartedAtUtc.
/// No audit columns beyond what's listed, verified against the real DDL.
/// Real invariant: ConfidenceScore, when set, must be in [0, 1]
/// (CK_ReinforcementLearning_AdvisoryRecommendation_ConfidenceScore).
/// </summary>
public sealed class AdvisoryRecommendation : Entity<AdvisoryRecommendationId>, IAggregateRoot
{
    private AdvisoryRecommendation(
        AdvisoryRecommendationId id, AdvisorySessionId advisorySessionId, RecommendationStatusId recommendationStatusId,
        StateDefinitionId stateDefinitionId, ActionDefinitionId recommendedActionDefinitionId,
        ActionDefinitionId? clampedActionDefinitionId, decimal? observedPowerPercent, decimal? targetPowerPercent,
        decimal? confidenceScore, bool wasClamped, string? clampReason, DateTime requestedAtUtc, DateTime? expiresAtUtc)
        : base(id)
    {
        AdvisorySessionId = advisorySessionId;
        RecommendationStatusId = recommendationStatusId;
        StateDefinitionId = stateDefinitionId;
        RecommendedActionDefinitionId = recommendedActionDefinitionId;
        ClampedActionDefinitionId = clampedActionDefinitionId;
        ObservedPowerPercent = observedPowerPercent;
        TargetPowerPercent = targetPowerPercent;
        ConfidenceScore = confidenceScore;
        WasClamped = wasClamped;
        ClampReason = clampReason;
        RequestedAtUtc = requestedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public AdvisorySessionId AdvisorySessionId { get; }

    public RecommendationStatusId RecommendationStatusId { get; }

    public StateDefinitionId StateDefinitionId { get; }

    /// <summary>The raw policy pick, before any safety clamp.</summary>
    public ActionDefinitionId RecommendedActionDefinitionId { get; }

    /// <summary>The action actually offered, after the safety clamp — nullable when no clamp applied.</summary>
    public ActionDefinitionId? ClampedActionDefinitionId { get; }

    public decimal? ObservedPowerPercent { get; }

    public decimal? TargetPowerPercent { get; }

    public decimal? ConfidenceScore { get; }

    public bool WasClamped { get; }

    public string? ClampReason { get; }

    public DateTime RequestedAtUtc { get; }

    public DateTime? ExpiresAtUtc { get; }

    public static AdvisoryRecommendation Create(
        AdvisoryRecommendationId id, AdvisorySessionId advisorySessionId, RecommendationStatusId recommendationStatusId,
        StateDefinitionId stateDefinitionId, ActionDefinitionId recommendedActionDefinitionId, DateTime requestedAtUtc,
        ActionDefinitionId? clampedActionDefinitionId = null, decimal? observedPowerPercent = null,
        decimal? targetPowerPercent = null, decimal? confidenceScore = null, bool wasClamped = false,
        string? clampReason = null, DateTime? expiresAtUtc = null)
    {
        if (confidenceScore is < 0 or > 1)
        {
            throw new ArgumentException(
                "ConfidenceScore must be between zero and one when set (CK_ReinforcementLearning_AdvisoryRecommendation_ConfidenceScore).",
                nameof(confidenceScore));
        }

        return new AdvisoryRecommendation(
            id, advisorySessionId, recommendationStatusId, stateDefinitionId, recommendedActionDefinitionId,
            clampedActionDefinitionId, observedPowerPercent, targetPowerPercent, confidenceScore, wasClamped,
            clampReason, requestedAtUtc, expiresAtUtc);
    }
}
