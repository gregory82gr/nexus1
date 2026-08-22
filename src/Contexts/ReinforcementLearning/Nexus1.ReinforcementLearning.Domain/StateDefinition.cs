using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// One discrete state within a StateSpace, e.g. a deviation/trend bin
/// (atlas C.11.2, C.11.5.2 query 3's own subject). (StateSpaceId, StateIndex)
/// and (StateSpaceId, Code) are both unique together. No audit columns at
/// all — the real DDL gives this table none (ADR-026).
/// </summary>
public sealed class StateDefinition : Entity<StateDefinitionId>, IAggregateRoot
{
    private StateDefinition(
        StateDefinitionId id, StateSpaceId stateSpaceId, int stateIndex, string code, string name,
        string? deviationBin, string? trendBin, bool isTerminal, int displayOrder)
        : base(id)
    {
        StateSpaceId = stateSpaceId;
        StateIndex = stateIndex;
        Code = code;
        Name = name;
        DeviationBin = deviationBin;
        TrendBin = trendBin;
        IsTerminal = isTerminal;
        DisplayOrder = displayOrder;
    }

    public StateSpaceId StateSpaceId { get; }

    public int StateIndex { get; }

    public string Code { get; }

    public string Name { get; }

    public string? DeviationBin { get; }

    public string? TrendBin { get; }

    public bool IsTerminal { get; }

    public int DisplayOrder { get; }

    public static StateDefinition Create(
        StateDefinitionId id, StateSpaceId stateSpaceId, int stateIndex, string code, string name,
        string? deviationBin = null, string? trendBin = null, bool isTerminal = false, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("StateDefinition code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("StateDefinition name must not be empty.", nameof(name));
        }

        return new StateDefinition(id, stateSpaceId, stateIndex, code, name, deviationBin, trendBin, isTerminal, displayOrder);
    }
}
