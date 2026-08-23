namespace Nexus1.Compliance.Application;

/// <summary>
/// Compliance had no Application layer at all before this (its entire prior
/// existence was write-side only: a message consumer opening
/// ComplianceReview rows from RootCause verdict events, ADR-011). This is
/// the first read-side query added, for the BFF's Audit &amp; Compliance
/// screen's Compliance half.
///
/// Same scoping shape as Audit's IAuditEvidenceFinder: keyed by
/// SourceAnalysisId (a RootCause analysis), not UnitId — there is no unit
/// scoping anywhere in ComplianceReview either.
/// </summary>
public interface IComplianceReviewFinder
{
    Task<IReadOnlyList<ComplianceReviewDto>> GetBySourceAnalysisIdAsync(long sourceAnalysisId, CancellationToken cancellationToken);
}
