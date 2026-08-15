namespace Nexus1.RootCause.Application;

public sealed record HypothesisDto(int HypothesisId, string HypothesisStatement, string Status, int EvidenceCount);

public sealed record AnalysisDto(
    long AnalysisId, int UnitId, long AlarmFloodId, string Status, string? Verdict, IReadOnlyList<HypothesisDto> Hypotheses);
