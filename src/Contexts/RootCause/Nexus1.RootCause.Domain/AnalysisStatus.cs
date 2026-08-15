namespace Nexus1.RootCause.Domain;

/// <summary>
/// Only Open/Closed modeled for Phase 1 (ADR-005) — the atlas's full
/// AnalysisStatus lookup codes weren't enumerated by this session's research.
/// </summary>
public enum AnalysisStatus
{
    Open,
    Closed,
}
