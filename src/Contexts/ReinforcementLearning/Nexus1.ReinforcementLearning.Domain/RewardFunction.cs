using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// The reward shaping formula a TrainingRun optimizes against (atlas
/// C.11.2). Full audit shape is NOT modeled in Domain — EF shadow
/// properties only.
/// </summary>
public sealed class RewardFunction : Entity<RewardFunctionId>, IAggregateRoot
{
    private RewardFunction(
        RewardFunctionId id, RewardFunctionTypeId rewardFunctionTypeId, string code, string name,
        string formulaText, decimal errorWeight, decimal movePenalty)
        : base(id)
    {
        RewardFunctionTypeId = rewardFunctionTypeId;
        Code = code;
        Name = name;
        FormulaText = formulaText;
        ErrorWeight = errorWeight;
        MovePenalty = movePenalty;
    }

    public RewardFunctionTypeId RewardFunctionTypeId { get; }

    public string Code { get; }

    public string Name { get; }

    public string FormulaText { get; }

    public decimal ErrorWeight { get; }

    public decimal MovePenalty { get; }

    public static RewardFunction Create(
        RewardFunctionId id, RewardFunctionTypeId rewardFunctionTypeId, string code, string name,
        string formulaText, decimal errorWeight = 100.0m, decimal movePenalty = 0.3m)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RewardFunction code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("RewardFunction name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(formulaText))
        {
            throw new ArgumentException("RewardFunction formulaText must not be empty.", nameof(formulaText));
        }

        return new RewardFunction(id, rewardFunctionTypeId, code, name, formulaText, errorWeight, movePenalty);
    }
}
