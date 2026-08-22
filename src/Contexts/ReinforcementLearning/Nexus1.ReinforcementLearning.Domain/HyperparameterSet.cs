using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// A pinned Q-learning hyperparameter configuration (atlas C.11.2) —
/// "given the same configuration and seed, you must be able to regenerate
/// the exact 175 numbers" (ADR-026, From_Trial_to_Policy Ch.11). No
/// internal FKs. Full audit shape is NOT modeled in Domain — EF shadow
/// properties only. Real invariants, each mirroring a CHECK constraint:
/// LearningRateAlpha in (0, 1], DiscountGamma/EpsilonStart/EpsilonEnd in
/// [0, 1], EpisodeCount/StepsPerEpisode greater than zero.
/// </summary>
public sealed class HyperparameterSet : Entity<HyperparameterSetId>, IAggregateRoot
{
    private HyperparameterSet(
        HyperparameterSetId id, string code, string name, decimal learningRateAlpha, decimal discountGamma,
        decimal epsilonStart, decimal epsilonEnd, decimal epsilonDecay, int episodeCount, int stepsPerEpisode,
        int? randomSeed)
        : base(id)
    {
        Code = code;
        Name = name;
        LearningRateAlpha = learningRateAlpha;
        DiscountGamma = discountGamma;
        EpsilonStart = epsilonStart;
        EpsilonEnd = epsilonEnd;
        EpsilonDecay = epsilonDecay;
        EpisodeCount = episodeCount;
        StepsPerEpisode = stepsPerEpisode;
        RandomSeed = randomSeed;
    }

    public string Code { get; }

    public string Name { get; }

    public decimal LearningRateAlpha { get; }

    public decimal DiscountGamma { get; }

    public decimal EpsilonStart { get; }

    public decimal EpsilonEnd { get; }

    public decimal EpsilonDecay { get; }

    public int EpisodeCount { get; }

    public int StepsPerEpisode { get; }

    public int? RandomSeed { get; }

    public static HyperparameterSet Create(
        HyperparameterSetId id, string code, string name, decimal learningRateAlpha, decimal discountGamma,
        decimal epsilonStart, decimal epsilonEnd, decimal epsilonDecay, int episodeCount, int stepsPerEpisode,
        int? randomSeed = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("HyperparameterSet code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("HyperparameterSet name must not be empty.", nameof(name));
        }

        if (learningRateAlpha <= 0 || learningRateAlpha > 1)
        {
            throw new ArgumentException(
                "LearningRateAlpha must be greater than zero and at most one (CK_ReinforcementLearning_HyperparameterSet_LearningRateAlpha).",
                nameof(learningRateAlpha));
        }

        if (discountGamma < 0 || discountGamma > 1)
        {
            throw new ArgumentException(
                "DiscountGamma must be between zero and one (CK_ReinforcementLearning_HyperparameterSet_DiscountGamma).",
                nameof(discountGamma));
        }

        if (epsilonStart < 0 || epsilonStart > 1)
        {
            throw new ArgumentException(
                "EpsilonStart must be between zero and one (CK_ReinforcementLearning_HyperparameterSet_EpsilonStart).",
                nameof(epsilonStart));
        }

        if (epsilonEnd < 0 || epsilonEnd > 1)
        {
            throw new ArgumentException(
                "EpsilonEnd must be between zero and one (CK_ReinforcementLearning_HyperparameterSet_EpsilonEnd).",
                nameof(epsilonEnd));
        }

        if (episodeCount <= 0)
        {
            throw new ArgumentException(
                "EpisodeCount must be greater than zero (CK_ReinforcementLearning_HyperparameterSet_EpisodeCount).",
                nameof(episodeCount));
        }

        if (stepsPerEpisode <= 0)
        {
            throw new ArgumentException(
                "StepsPerEpisode must be greater than zero (CK_ReinforcementLearning_HyperparameterSet_StepsPerEpisode).",
                nameof(stepsPerEpisode));
        }

        return new HyperparameterSet(
            id, code, name, learningRateAlpha, discountGamma, epsilonStart, epsilonEnd, epsilonDecay, episodeCount,
            stepsPerEpisode, randomSeed);
    }
}
