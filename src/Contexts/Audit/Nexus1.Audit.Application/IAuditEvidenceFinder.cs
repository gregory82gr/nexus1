namespace Nexus1.Audit.Application;

/// <summary>
/// Audit had no Application layer at all before this (its entire prior
/// existence was write-side only: a message consumer appending
/// AuditEvidenceRecord rows from RootCause verdict events, ADR-010). This is
/// the first read-side query added, for the BFF's Audit &amp; Compliance screen.
///
/// Named gap: Audit's domain is a single append-only evidence ledger keyed
/// by SourceAnalysisId (a RootCause analysis) — there is no UnitId anywhere
/// in AuditEvidenceRecord, and no general "who changed what record" system
/// audit-trail concept. The realistic scoping key that actually exists is
/// per-analysis, not per-unit or fleet-wide-unscoped.
/// </summary>
public interface IAuditEvidenceFinder
{
    Task<IReadOnlyList<AuditEvidenceRecordDto>> GetBySourceAnalysisIdAsync(long sourceAnalysisId, CancellationToken cancellationToken);
}
