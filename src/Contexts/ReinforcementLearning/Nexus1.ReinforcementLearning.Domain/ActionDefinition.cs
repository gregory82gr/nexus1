using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// One discrete action within an ActionSpace, e.g. a control rod move
/// (atlas C.11.2, C.11.5.2 query 3's own subject). (ActionSpaceId,
/// ActionIndex) and (ActionSpaceId, Code) are both unique together. No
/// audit columns at all — the real DDL gives this table none (ADR-026).
/// </summary>
public sealed class ActionDefinition : Entity<ActionDefinitionId>, IAggregateRoot
{
    private ActionDefinition(
        ActionDefinitionId id, ActionSpaceId actionSpaceId, int actionIndex, string code, string name,
        decimal actionValue, bool isNoOp, int displayOrder)
        : base(id)
    {
        ActionSpaceId = actionSpaceId;
        ActionIndex = actionIndex;
        Code = code;
        Name = name;
        ActionValue = actionValue;
        IsNoOp = isNoOp;
        DisplayOrder = displayOrder;
    }

    public ActionSpaceId ActionSpaceId { get; }

    public int ActionIndex { get; }

    public string Code { get; }

    public string Name { get; }

    public decimal ActionValue { get; }

    public bool IsNoOp { get; }

    public int DisplayOrder { get; }

    public static ActionDefinition Create(
        ActionDefinitionId id, ActionSpaceId actionSpaceId, int actionIndex, string code, string name,
        decimal actionValue, bool isNoOp = false, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("ActionDefinition code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("ActionDefinition name must not be empty.", nameof(name));
        }

        return new ActionDefinition(id, actionSpaceId, actionIndex, code, name, actionValue, isNoOp, displayOrder);
    }
}
