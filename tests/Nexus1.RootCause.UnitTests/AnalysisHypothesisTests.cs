using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.UnitTests;

public class AnalysisHypothesisTests
{
    private static readonly DateTime RecordedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RejectHypothesis_via_the_analysis_raises_HypothesisRejected_with_the_reason()
    {
        var analysis = RootCauseAnalysis.Open(new RootCauseAnalysisId(1), new UnitId(1), new AlarmFloodId(1), "operator.1", RecordedAtUtc);
        analysis.AddHypothesis(new AnalysisHypothesisId(1), "Loose fitting on primary loop.");
        analysis.ClearDomainEvents();
        var rejectedAtUtc = RecordedAtUtc.AddMinutes(10);

        analysis.RejectHypothesis(new AnalysisHypothesisId(1), "Ruled out by inspection.", rejectedAtUtc);

        var hypothesis = Assert.Single(analysis.Hypotheses);
        Assert.Equal(HypothesisStatus.Rejected, hypothesis.Status);
        Assert.Equal("Ruled out by inspection.", hypothesis.RejectionReason);
        Assert.Equal(rejectedAtUtc, hypothesis.RejectedAtUtc);

        var rejected = Assert.IsType<HypothesisRejected>(Assert.Single(analysis.DomainEvents));
        Assert.Equal(analysis.Id, rejected.AnalysisId);
        Assert.Equal(new AnalysisHypothesisId(1), rejected.HypothesisId);
        Assert.Equal("Ruled out by inspection.", rejected.Reason);
        Assert.Equal(rejectedAtUtc, rejected.RejectedAtUtc);
    }

    [Fact]
    public void AddEvidence_via_the_analysis_attaches_evidence_to_the_right_hypothesis()
    {
        var analysis = RootCauseAnalysis.Open(new RootCauseAnalysisId(1), new UnitId(1), new AlarmFloodId(1), "operator.1", RecordedAtUtc);
        analysis.AddHypothesis(new AnalysisHypothesisId(1), "Loose fitting on primary loop.");
        analysis.AddHypothesis(new AnalysisHypothesisId(2), "Sensor drift.");

        analysis.AddEvidence(new AnalysisHypothesisId(2), new HypothesisEvidenceId(1), "Calibration log.", RecordedAtUtc);

        var hypothesisOne = analysis.Hypotheses.Single(h => h.Id == new AnalysisHypothesisId(1));
        var hypothesisTwo = analysis.Hypotheses.Single(h => h.Id == new AnalysisHypothesisId(2));
        Assert.Empty(hypothesisOne.Evidence);
        var evidence = Assert.Single(hypothesisTwo.Evidence);
        Assert.Equal("Calibration log.", evidence.Description);
        Assert.Equal(RecordedAtUtc, evidence.RecordedAtUtc);
    }
}
