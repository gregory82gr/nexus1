using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.UnitTests;

public class RootCauseAnalysisTests
{
    private static readonly DateTime OpenedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private static RootCauseAnalysis OpenSample() =>
        RootCauseAnalysis.Open(new RootCauseAnalysisId(1), new UnitId(1), new AlarmFloodId(1), "operator.1", OpenedAtUtc);

    [Fact]
    public void Open_starts_in_open_status_and_raises_RootCauseAnalysisOpened_event()
    {
        var analysis = OpenSample();

        Assert.Equal(AnalysisStatus.Open, analysis.Status);
        Assert.Empty(analysis.Hypotheses);
        var opened = Assert.IsType<RootCauseAnalysisOpened>(Assert.Single(analysis.DomainEvents));
        Assert.Equal(analysis.Id, opened.AnalysisId);
        Assert.Equal(analysis.UnitId, opened.UnitId);
        Assert.Equal(analysis.AlarmFloodId, opened.AlarmFloodId);
    }

    [Fact]
    public void AddHypothesis_while_open_succeeds()
    {
        var analysis = OpenSample();

        analysis.AddHypothesis(new AnalysisHypothesisId(1), "Loose fitting on primary loop.");

        var hypothesis = Assert.Single(analysis.Hypotheses);
        Assert.Equal("Loose fitting on primary loop.", hypothesis.HypothesisStatement);
        Assert.Equal(HypothesisStatus.Proposed, hypothesis.Status);
    }

    [Fact]
    public void Close_without_any_evidence_throws()
    {
        var analysis = OpenSample();
        analysis.AddHypothesis(new AnalysisHypothesisId(1), "Loose fitting on primary loop.");

        var ex = Assert.Throws<InvalidOperationException>(() => analysis.Close("Confirmed", "operator.1", OpenedAtUtc));
        Assert.Equal("A root-cause case cannot close without evidence.", ex.Message);
    }

    [Fact]
    public void Close_with_all_hypotheses_rejected_throws()
    {
        var analysis = OpenSample();
        analysis.AddHypothesis(new AnalysisHypothesisId(1), "Loose fitting on primary loop.");
        analysis.AddEvidence(new AnalysisHypothesisId(1), new HypothesisEvidenceId(1), "Inspection photo.", OpenedAtUtc);
        analysis.RejectHypothesis(new AnalysisHypothesisId(1), "Ruled out by inspection.", OpenedAtUtc);

        var ex = Assert.Throws<InvalidOperationException>(() => analysis.Close("Confirmed", "operator.1", OpenedAtUtc));
        Assert.Equal("At least one hypothesis must remain supported or accepted.", ex.Message);
    }

    [Fact]
    public void Close_with_evidence_and_a_non_rejected_hypothesis_succeeds_and_raises_event()
    {
        var analysis = OpenSample();
        analysis.AddHypothesis(new AnalysisHypothesisId(1), "Loose fitting on primary loop.");
        analysis.AddEvidence(new AnalysisHypothesisId(1), new HypothesisEvidenceId(1), "Inspection photo.", OpenedAtUtc);
        analysis.ClearDomainEvents();
        var closedAtUtc = OpenedAtUtc.AddHours(2);

        analysis.Close("Loose fitting confirmed as cause.", "operator.2", closedAtUtc);

        Assert.Equal(AnalysisStatus.Closed, analysis.Status);
        Assert.Equal("Loose fitting confirmed as cause.", analysis.Verdict);
        Assert.Equal("operator.2", analysis.ClosedBy);
        Assert.Equal(closedAtUtc, analysis.ClosedAtUtc);
        var closed = Assert.IsType<RootCauseAnalysisClosed>(Assert.Single(analysis.DomainEvents));
        Assert.Equal(analysis.Id, closed.AnalysisId);
        Assert.Equal("Loose fitting confirmed as cause.", closed.Verdict);
    }

    [Fact]
    public void Mutating_a_closed_analysis_throws()
    {
        var analysis = OpenSample();
        analysis.AddHypothesis(new AnalysisHypothesisId(1), "Loose fitting on primary loop.");
        analysis.AddEvidence(new AnalysisHypothesisId(1), new HypothesisEvidenceId(1), "Inspection photo.", OpenedAtUtc);
        analysis.Close("Loose fitting confirmed as cause.", "operator.2", OpenedAtUtc);

        var ex1 = Assert.Throws<InvalidOperationException>(() => analysis.AddHypothesis(new AnalysisHypothesisId(2), "Another cause."));
        Assert.Equal("Closed cases cannot be changed.", ex1.Message);

        var ex2 = Assert.Throws<InvalidOperationException>(() =>
            analysis.AddEvidence(new AnalysisHypothesisId(1), new HypothesisEvidenceId(2), "More evidence.", OpenedAtUtc));
        Assert.Equal("Closed cases cannot be changed.", ex2.Message);

        var ex3 = Assert.Throws<InvalidOperationException>(() =>
            analysis.RejectHypothesis(new AnalysisHypothesisId(1), "Too late.", OpenedAtUtc));
        Assert.Equal("Closed cases cannot be changed.", ex3.Message);
    }

    [Fact]
    public void AddEvidence_for_unknown_hypothesis_throws()
    {
        var analysis = OpenSample();

        Assert.Throws<InvalidOperationException>(() =>
            analysis.AddEvidence(new AnalysisHypothesisId(99), new HypothesisEvidenceId(1), "Evidence.", OpenedAtUtc));
    }
}
