using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// One learned (state, action) -&gt; Q-value cell (atlas C.11.2, C.11.5.2
/// query 2's own subject). (QTableId, StateDefinitionId, ActionDefinitionId)
/// is unique together. No audit columns at all — the real DDL gives this
/// table none (ADR-026).
/// </summary>
public sealed class QTableEntry : Entity<QTableEntryId>, IAggregateRoot
{
    private QTableEntry(
        QTableEntryId id, QTableId qTableId, StateDefinitionId stateDefinitionId, ActionDefinitionId actionDefinitionId,
        decimal qValue, int visitCount, DateTime? lastUpdatedAtUtc)
        : base(id)
    {
        QTableId = qTableId;
        StateDefinitionId = stateDefinitionId;
        ActionDefinitionId = actionDefinitionId;
        QValue = qValue;
        VisitCount = visitCount;
        LastUpdatedAtUtc = lastUpdatedAtUtc;
    }

    public QTableId QTableId { get; }

    public StateDefinitionId StateDefinitionId { get; }

    public ActionDefinitionId ActionDefinitionId { get; }

    public decimal QValue { get; }

    public int VisitCount { get; }

    public DateTime? LastUpdatedAtUtc { get; }

    public static QTableEntry Create(
        QTableEntryId id, QTableId qTableId, StateDefinitionId stateDefinitionId, ActionDefinitionId actionDefinitionId,
        decimal qValue, int visitCount = 0, DateTime? lastUpdatedAtUtc = null)
    {
        return new QTableEntry(id, qTableId, stateDefinitionId, actionDefinitionId, qValue, visitCount, lastUpdatedAtUtc);
    }
}
