namespace Nexus1.Compliance.Application;

/// <summary>
/// Shaped for the BFF's Compliance half of the Audit &amp; Compliance screen.
/// SourceAnalysisId is an opaque long reference to a RootCause analysis;
/// RootCause stays out-of-process (ADR-001), so this cannot be resolved
/// into a human-readable case name here — the id is surfaced as-is.
///
/// Named gap: State will read "Pending" for every real row that exists
/// today, and can never read anything else — ComplianceReviewState (the
/// domain enum) has exactly one member, and ComplianceReview exposes no
/// method that ever transitions it. Review assignment, findings, and a
/// decision are the book's own named-future authority (ch.34, 34-AL) —
/// not implemented yet, not merely absent from this DTO. A "Compliance
/// status/findings" screen showing pass/fail or open findings would be
/// showing something that does not exist in this codebase yet.
/// </summary>
public sealed record ComplianceReviewDto(
    Guid ComplianceReviewId,
    long SourceAnalysisId,
    string Verdict,
    string State,
    DateTime OpenedAtUtc);
