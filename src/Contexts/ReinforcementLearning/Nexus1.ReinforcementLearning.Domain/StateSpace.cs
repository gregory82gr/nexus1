using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// The discretized state space a TrainingRun learns over (atlas C.11.2).
/// Full audit shape is NOT modeled in Domain — EF shadow properties only.
/// Real invariant: DimensionCount must be greater than zero
/// (CK_ReinforcementLearning_StateSpace_DimensionCount).
/// </summary>
public sealed class StateSpace : Entity<StateSpaceId>, IAggregateRoot
{
    private StateSpace(
        StateSpaceId id, StateSpaceTypeId stateSpaceTypeId, string code, string name, string? description,
        int dimensionCount, bool isDiscrete)
        : base(id)
    {
        StateSpaceTypeId = stateSpaceTypeId;
        Code = code;
        Name = name;
        Description = description;
        DimensionCount = dimensionCount;
        IsDiscrete = isDiscrete;
    }

    public StateSpaceTypeId StateSpaceTypeId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public int DimensionCount { get; }

    public bool IsDiscrete { get; }

    public static StateSpace Create(
        StateSpaceId id, StateSpaceTypeId stateSpaceTypeId, string code, string name, int dimensionCount,
        string? description = null, bool isDiscrete = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("StateSpace code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("StateSpace name must not be empty.", nameof(name));
        }

        if (dimensionCount <= 0)
        {
            throw new ArgumentException(
                "DimensionCount must be greater than zero (CK_ReinforcementLearning_StateSpace_DimensionCount).",
                nameof(dimensionCount));
        }

        return new StateSpace(id, stateSpaceTypeId, code, name, description, dimensionCount, isDiscrete);
    }
}
