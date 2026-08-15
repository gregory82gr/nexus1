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

    private RootCauseAnalysis(RootCauseAnalysisId id, UnitId unitId, AlarmFloodId alarmFloodId, string openedBy, DateTime openedAtUtc)
        : base(id)
    {
        UnitId = unitId;
        AlarmFloodId = alarmFloodId;
        Status = AnalysisStatus.Open;
        OpenedBy = openedBy;
        OpenedAtUtc = openedAtUtc;
    }

    public UnitId UnitId { get; }

    public AlarmFloodId AlarmFloodId { get; }

    public AnalysisStatus Status { get; private set; }

    public string OpenedBy { get; }

    public DateTime OpenedAtUtc { get; }

    public string? Verdict { get; private set; }

    public string? ClosedBy { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public IReadOnlyCollection<AnalysisHypothesis> Hypotheses => _hypotheses.AsReadOnly();

    public static RootCauseAnalysis Open(
        RootCauseAnalysisId id, UnitId unitId, AlarmFloodId alarmFloodId, string openedBy, DateTime openedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(openedBy))
        {
            throw new ArgumentException("OpenedBy must not be empty.", nameof(openedBy));
        }

        var analysis = new RootCauseAnalysis(id, unitId, alarmFloodId, openedBy, openedAtUtc);
        analysis.AddDomainEvent(new RootCauseAnalysisOpened(id, unitId, alarmFloodId, openedAtUtc));
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
            throw new InvalidOperationException("Closed cases cannot be changed.");
        }
    }
}
