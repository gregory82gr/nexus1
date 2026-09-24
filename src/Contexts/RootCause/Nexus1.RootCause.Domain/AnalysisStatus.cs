namespace Nexus1.RootCause.Domain;

/// <summary>
/// Open/Closed modeled for Phase 1 (ADR-005); Inconclusive added (ADR-039) as a
/// third TERMINAL state so a case can be honestly recorded as "investigated, cannot
/// conclude" — the pre-existing human-lifecycle gap where Close() requires a verdict,
/// so an unresolvable case could otherwise only stay Open forever. The atlas's full
/// AnalysisStatus lookup codes still weren't enumerated by this session's research;
/// these are the three this project's lifecycle actually uses.
/// </summary>
public enum AnalysisStatus
{
    Open,
    Closed,
    Inconclusive,
}
