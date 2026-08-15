namespace Nexus1.RootCause.Domain;

/// <summary>
/// Only Proposed/Rejected modeled for Phase 1 (ADR-005) — sufficient for the
/// book's stated close invariant without inventing unconfirmed atlas lookup
/// codes for an "Accepted"/"Supported" status.
/// </summary>
public enum HypothesisStatus
{
    Proposed,
    Rejected,
}
