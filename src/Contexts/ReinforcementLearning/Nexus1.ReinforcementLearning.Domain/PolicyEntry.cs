using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// One state's best-action row in the readable policy grid — Chapter 9's
/// grid, made queryable (BestQValue, SecondBestQValue, ActionMargin, IsTie)
/// (atlas C.11.2, C.11.5.2 queries 1 and 3). (PolicyId, StateDefinitionId)
/// is unique together. No audit columns at all — the real DDL gives this
/// table none (ADR-026).
/// </summary>
public sealed class PolicyEntry : Entity<PolicyEntryId>, IAggregateRoot
{
    private PolicyEntry(
        PolicyEntryId id, PolicyId policyId, StateDefinitionId stateDefinitionId,
        ActionDefinitionId bestActionDefinitionId, decimal bestQValue, decimal? secondBestQValue,
        decimal? actionMargin, bool isTie)
        : base(id)
    {
        PolicyId = policyId;
        StateDefinitionId = stateDefinitionId;
        BestActionDefinitionId = bestActionDefinitionId;
        BestQValue = bestQValue;
        SecondBestQValue = secondBestQValue;
        ActionMargin = actionMargin;
        IsTie = isTie;
    }

    public PolicyId PolicyId { get; }

    public StateDefinitionId StateDefinitionId { get; }

    public ActionDefinitionId BestActionDefinitionId { get; }

    public decimal BestQValue { get; }

    public decimal? SecondBestQValue { get; }

    public decimal? ActionMargin { get; }

    public bool IsTie { get; }

    public static PolicyEntry Create(
        PolicyEntryId id, PolicyId policyId, StateDefinitionId stateDefinitionId,
        ActionDefinitionId bestActionDefinitionId, decimal bestQValue, decimal? secondBestQValue = null,
        decimal? actionMargin = null, bool isTie = false)
    {
        return new PolicyEntry(
            id, policyId, stateDefinitionId, bestActionDefinitionId, bestQValue, secondBestQValue, actionMargin, isTie);
    }
}
