using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReinforcementLearning.Domain;

/// <summary>
/// The raw learned Q-table snapshot for a TrainingRun (atlas C.11.2,
/// C.11.5.2 query 2's own subject: "A final Q-table should contain 175
/// state-action values"). SnapshotAtUtc has a SQL DEFAULT
/// (SYSUTCDATETIME()) but is still a required constructor param, matching
/// every prior sector's "SQL default exists but Domain still always
/// supplies it" pattern. Audit shape is narrower than most: CreatedAtUtc/
/// CreatedBy/RowVersion only — no ModifiedAtUtc/ModifiedBy/IsDeleted
/// (verified against the real DDL). Not modeled in Domain — EF shadow
/// properties only. Real invariant: EntryCount must be greater than zero
/// (CK_ReinforcementLearning_QTable_EntryCount).
/// </summary>
public sealed class QTable : Entity<QTableId>, IAggregateRoot
{
    private QTable(
        QTableId id, TrainingRunId trainingRunId, StateSpaceId stateSpaceId, ActionSpaceId actionSpaceId,
        string code, DateTime snapshotAtUtc, int entryCount, bool isFinal)
        : base(id)
    {
        TrainingRunId = trainingRunId;
        StateSpaceId = stateSpaceId;
        ActionSpaceId = actionSpaceId;
        Code = code;
        SnapshotAtUtc = snapshotAtUtc;
        EntryCount = entryCount;
        IsFinal = isFinal;
    }

    public TrainingRunId TrainingRunId { get; }

    public StateSpaceId StateSpaceId { get; }

    public ActionSpaceId ActionSpaceId { get; }

    public string Code { get; }

    public DateTime SnapshotAtUtc { get; }

    public int EntryCount { get; }

    public bool IsFinal { get; }

    public static QTable Create(
        QTableId id, TrainingRunId trainingRunId, StateSpaceId stateSpaceId, ActionSpaceId actionSpaceId,
        string code, DateTime snapshotAtUtc, int entryCount, bool isFinal = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("QTable code must not be empty.", nameof(code));
        }

        if (entryCount <= 0)
        {
            throw new ArgumentException(
                "EntryCount must be greater than zero (CK_ReinforcementLearning_QTable_EntryCount).",
                nameof(entryCount));
        }

        return new QTable(id, trainingRunId, stateSpaceId, actionSpaceId, code, snapshotAtUtc, entryCount, isFinal);
    }
}
