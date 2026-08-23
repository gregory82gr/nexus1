namespace Nexus1.Audit.Application;

/// <summary>
/// Shaped for the BFF's Audit half of the Audit &amp; Compliance screen.
/// Deliberately omits the raw envelope bytes (large, opaque binary, not
/// screen-appropriate) — exposes the SHA-256 as a hex string instead, since
/// that is what a screen would show as the tamper-evidence fingerprint.
/// SourceAnalysisId is an opaque long reference to a RootCause analysis;
/// RootCause stays out-of-process (ADR-001), so this cannot be resolved
/// into a human-readable case name here — the id is surfaced as-is.
/// </summary>
public sealed record AuditEvidenceRecordDto(
    Guid AuditEvidenceId,
    long SourceAnalysisId,
    string EventType,
    int SchemaVersion,
    Guid CorrelationId,
    Guid? CausationId,
    string EnvelopeSha256Hex,
    DateTime OccurredAtUtc,
    DateTime RecordedAtUtc);
