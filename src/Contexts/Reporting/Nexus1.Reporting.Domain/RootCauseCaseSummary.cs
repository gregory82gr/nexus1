using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Reporting.Domain;

/// <summary>
/// Reporting-owned "delayed truth" projection per Executable Assets 35-D/
/// 35-G/35-H, reduced to this project's actual two reducers and no
/// stream-position watermark (ADR-012). ApplyOpened creates the row from
/// RootCauseCaseOpenedV1; ApplyVerdictIssued updates it from
/// RootCauseVerdictIssuedV1 — a read-model observation, never a RootCause
/// invariant source (book's own "READ-MODEL LIMIT", 35-D).
/// </summary>
public sealed class RootCauseCaseSummary : Entity<RootCauseCaseSummaryId>, IAggregateRoot
{
    private RootCauseCaseSummary(
        RootCauseCaseSummaryId id, int unitId, long alarmFloodId, DateTime openedAtUtc,
        DateTime lastAppliedAtUtc, Guid lastAppliedMessageId)
        : base(id)
    {
        UnitId = unitId;
        AlarmFloodId = alarmFloodId;
        Status = ReportingCaseStatus.Open;
        OpenedAtUtc = openedAtUtc;
        LastAppliedAtUtc = lastAppliedAtUtc;
        LastAppliedMessageId = lastAppliedMessageId;
    }

    public int UnitId { get; }

    public long AlarmFloodId { get; }

    public ReportingCaseStatus Status { get; private set; }

    public string? Verdict { get; private set; }

    /// <summary>Why the case was marked Inconclusive (ADR-039) — set only by ApplyInconclusive, never alongside a Verdict.</summary>
    public string? Reason { get; private set; }

    public DateTime OpenedAtUtc { get; }

    public DateTime? VerdictIssuedAtUtc { get; private set; }

    /// <summary>When the case reached a terminal read-model status (VerdictIssued or Inconclusive) — a generalized terminal timestamp (ADR-039).</summary>
    public DateTime? FinalizedAtUtc { get; private set; }

    public DateTime LastAppliedAtUtc { get; private set; }

    public Guid LastAppliedMessageId { get; private set; }

    public static RootCauseCaseSummary ApplyOpened(
        RootCauseCaseSummaryId id, int unitId, long alarmFloodId, DateTime openedAtUtc,
        DateTime appliedAtUtc, Guid messageId)
        => new(id, unitId, alarmFloodId, openedAtUtc, appliedAtUtc, messageId);

    public void ApplyVerdictIssued(string verdict, DateTime verdictIssuedAtUtc, DateTime appliedAtUtc, Guid messageId)
    {
        if (Status == ReportingCaseStatus.VerdictIssued)
        {
            throw new InvalidOperationException($"Case {Id.Value} already has an issued verdict.");
        }

        if (string.IsNullOrWhiteSpace(verdict))
        {
            throw new ArgumentException("Verdict must not be empty.", nameof(verdict));
        }

        Status = ReportingCaseStatus.VerdictIssued;
        Verdict = verdict;
        VerdictIssuedAtUtc = verdictIssuedAtUtc;
        FinalizedAtUtc = verdictIssuedAtUtc;
        LastAppliedAtUtc = appliedAtUtc;
        LastAppliedMessageId = messageId;
    }

    /// <summary>
    /// Reduces RootCauseCaseInconclusiveV1 (ADR-039): a terminal status carrying a
    /// reason, never a verdict. Idempotency-guarded the same way as ApplyVerdictIssued —
    /// once the case is terminal, a replay is a no-op via the caller's status check;
    /// this method refuses to overwrite an already-terminal case defensively.
    /// </summary>
    public void ApplyInconclusive(string reason, DateTime decidedAtUtc, DateTime appliedAtUtc, Guid messageId)
    {
        if (Status != ReportingCaseStatus.Open)
        {
            throw new InvalidOperationException($"Case {Id.Value} is already finalized.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason must not be empty.", nameof(reason));
        }

        Status = ReportingCaseStatus.Inconclusive;
        Reason = reason;
        FinalizedAtUtc = decidedAtUtc;
        LastAppliedAtUtc = appliedAtUtc;
        LastAppliedMessageId = messageId;
    }
}
