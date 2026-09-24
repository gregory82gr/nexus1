using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RootCause.Domain;

/// <summary>
/// Aggregate root named after the Schema Atlas's real RootCauseAnalysis
/// table, not the book's RootCauseCase (ADR-005) — the book's own worked
/// example is reconciled here into one canonical, minimal Phase-1 shape.
/// Every mutating method throws once Closed, generalizing the book's
/// "Closed cases cannot be changed" beyond just Close itself.
/// </summary>
public sealed class RootCauseAnalysis : Entity<RootCauseAnalysisId>, IAggregateRoot
{
    private readonly List<AnalysisHypothesis> _hypotheses = [];

    private RootCauseAnalysis(
        RootCauseAnalysisId id, UnitId unitId, AlarmFloodId? alarmFloodId, string openedBy, DateTime openedAtUtc,
        DateTime? alarmFloodStartedAtUtc)
        : base(id)
    {
        UnitId = unitId;
        AlarmFloodId = alarmFloodId;
        Status = AnalysisStatus.Open;
        OpenedBy = openedBy;
        OpenedAtUtc = openedAtUtc;
        AlarmFloodStartedAtUtc = alarmFloodStartedAtUtc;
    }

    public UnitId UnitId { get; }

    /// <summary>
    /// The originating alarm flood, or null for a provenance-originated case (ADR-040):
    /// a case that did not begin from a flood at all (no alarm, no telemetry — found by an
    /// out-of-band provenance audit). Not a fake/sentinel id; genuinely absent.
    /// </summary>
    public AlarmFloodId? AlarmFloodId { get; }

    public AnalysisStatus Status { get; private set; }

    public string OpenedBy { get; }

    public DateTime OpenedAtUtc { get; }

    /// <summary>
    /// The originating AlarmFlood's own StartedAtUtc (ch.52 52-T's "flood
    /// detected" milestone) — null when this analysis was opened through the
    /// manual OpenAnalysisCommand path, which receives only an AlarmFloodId,
    /// not the flood's timestamp (no cross-context read back to
    /// AlarmManagementDb exists to backfill it). Populated on the real
    /// auto-open production path (AlarmFloodMessageHandler), which already
    /// has it from the AlarmFloodDetectedV1 payload it is processing.
    /// Workflow-duration metrics are recorded only when this is present —
    /// never fabricated from OpenedAtUtc as a stand-in.
    /// </summary>
    public DateTime? AlarmFloodStartedAtUtc { get; }

    public string? Verdict { get; private set; }

    public string? ClosedBy { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    /// <summary>Why the case was marked Inconclusive (ADR-039) — non-null exactly when Status == Inconclusive; never accompanied by a Verdict.</summary>
    public string? InconclusiveReason { get; private set; }

    public string? DecidedBy { get; private set; }

    public DateTime? DecidedAtUtc { get; private set; }

    public IReadOnlyCollection<AnalysisHypothesis> Hypotheses => _hypotheses.AsReadOnly();

    public static RootCauseAnalysis Open(
        RootCauseAnalysisId id, UnitId unitId, AlarmFloodId alarmFloodId, string openedBy, DateTime openedAtUtc,
        DateTime? alarmFloodStartedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(openedBy))
        {
            throw new ArgumentException("OpenedBy must not be empty.", nameof(openedBy));
        }

        var analysis = new RootCauseAnalysis(id, unitId, alarmFloodId, openedBy, openedAtUtc, alarmFloodStartedAtUtc);
        analysis.AddDomainEvent(new RootCauseAnalysisOpened(id, unitId, alarmFloodId, openedAtUtc));
        return analysis;
    }

    /// <summary>
    /// Opens a provenance-originated case (ADR-040) — one that did NOT begin from an alarm
    /// flood (no alarm, no telemetry), e.g. a fault exposed only by a provenance/QA audit.
    /// AlarmFloodId is genuinely absent (null), not a sentinel; there is no flood timestamp
    /// either. Such a case is expected to terminate as Inconclusive: the engine that thrives
    /// on floods has no reach here (EVT-2026-0420).
    /// </summary>
    public static RootCauseAnalysis OpenForProvenance(
        RootCauseAnalysisId id, UnitId unitId, string openedBy, DateTime openedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(openedBy))
        {
            throw new ArgumentException("OpenedBy must not be empty.", nameof(openedBy));
        }

        var analysis = new RootCauseAnalysis(id, unitId, alarmFloodId: null, openedBy, openedAtUtc, alarmFloodStartedAtUtc: null);
        analysis.AddDomainEvent(new RootCauseAnalysisOpened(id, unitId, AlarmFloodId: null, openedAtUtc));
        return analysis;
    }

    public void AddHypothesis(AnalysisHypothesisId hypothesisId, string hypothesisStatement)
    {
        EnsureOpen();
        _hypotheses.Add(new AnalysisHypothesis(hypothesisId, hypothesisStatement));
    }

    public void AddEvidence(AnalysisHypothesisId hypothesisId, HypothesisEvidenceId evidenceId, string description, DateTime recordedAtUtc)
    {
        EnsureOpen();
        FindHypothesis(hypothesisId).AddEvidence(evidenceId, description, recordedAtUtc);
    }

    public void RejectHypothesis(AnalysisHypothesisId hypothesisId, string reason, DateTime rejectedAtUtc)
    {
        EnsureOpen();
        var hypothesis = FindHypothesis(hypothesisId);
        hypothesis.Reject(reason, rejectedAtUtc);
        AddDomainEvent(new HypothesisRejected(Id, hypothesisId, reason, rejectedAtUtc));
    }

    public void Close(string verdict, string closedBy, DateTime closedAtUtc)
    {
        EnsureOpen();

        if (string.IsNullOrWhiteSpace(verdict))
        {
            throw new ArgumentException("Verdict must not be empty.", nameof(verdict));
        }

        if (!_hypotheses.Any(h => h.Evidence.Count > 0))
        {
            throw new InvalidOperationException("A root-cause case cannot close without evidence.");
        }

        if (_hypotheses.All(h => h.Status == HypothesisStatus.Rejected))
        {
            throw new InvalidOperationException("At least one hypothesis must remain supported or accepted.");
        }

        Verdict = verdict;
        ClosedBy = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = AnalysisStatus.Closed;
        AddDomainEvent(new RootCauseAnalysisClosed(Id, verdict, closedAtUtc));
    }

    /// <summary>
    /// Terminally records "investigated, cannot conclude" (ADR-039). Unlike Close(),
    /// it requires only a reason — no verdict, no hypothesis, no evidence — because the
    /// point is to capture an unresolvable case honestly rather than leave it Open
    /// forever. Verdict stays null; the case becomes immutable like Closed.
    /// </summary>
    public void MarkInconclusive(string reason, string decidedBy, DateTime decidedAtUtc)
    {
        EnsureOpen();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("An inconclusive case must record a reason.", nameof(reason));
        }

        InconclusiveReason = reason;
        DecidedBy = decidedBy;
        DecidedAtUtc = decidedAtUtc;
        Status = AnalysisStatus.Inconclusive;
        AddDomainEvent(new RootCauseAnalysisMarkedInconclusive(Id, reason, decidedAtUtc));
    }

    private AnalysisHypothesis FindHypothesis(AnalysisHypothesisId hypothesisId)
    {
        var hypothesis = _hypotheses.SingleOrDefault(h => h.Id == hypothesisId);
        if (hypothesis is null)
        {
            throw new InvalidOperationException($"Hypothesis {hypothesisId} does not belong to this analysis.");
        }

        return hypothesis;
    }

    private void EnsureOpen()
    {
        if (Status != AnalysisStatus.Open)
        {
            // Generalized for both terminal states (Closed and Inconclusive, ADR-039).
            throw new InvalidOperationException("A finalized case cannot be changed.");
        }
    }
}
