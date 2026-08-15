using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RootCause.Domain;

/// <summary>
/// Child entity of RootCauseAnalysis, not its own aggregate (ADR-005) — both
/// sources agree hypotheses live inside the case/analysis consistency
/// boundary. Mutating members are internal: outside code goes through
/// RootCauseAnalysis, matching the book's own "outside code does not change
/// Evidence or Verdict directly" rule.
/// </summary>
public sealed class AnalysisHypothesis : Entity<AnalysisHypothesisId>
{
    private readonly List<HypothesisEvidence> _evidence = [];

    internal AnalysisHypothesis(AnalysisHypothesisId id, string hypothesisStatement)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(hypothesisStatement))
        {
            throw new ArgumentException("Hypothesis statement must not be empty.", nameof(hypothesisStatement));
        }

        HypothesisStatement = hypothesisStatement;
        Status = HypothesisStatus.Proposed;
    }

    public string HypothesisStatement { get; }

    public HypothesisStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTime? RejectedAtUtc { get; private set; }

    public IReadOnlyCollection<HypothesisEvidence> Evidence => _evidence.AsReadOnly();

    internal void AddEvidence(HypothesisEvidenceId id, string description, DateTime recordedAtUtc)
    {
        _evidence.Add(new HypothesisEvidence(id, description, recordedAtUtc));
    }

    internal void Reject(string reason, DateTime rejectedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Rejection reason must not be empty.", nameof(reason));
        }

        Status = HypothesisStatus.Rejected;
        RejectionReason = reason;
        RejectedAtUtc = rejectedAtUtc;
    }
}
