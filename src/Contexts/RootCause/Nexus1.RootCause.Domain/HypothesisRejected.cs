namespace Nexus1.RootCause.Domain;

public sealed record HypothesisRejected(RootCauseAnalysisId AnalysisId, AnalysisHypothesisId HypothesisId, string Reason, DateTime RejectedAtUtc);
