namespace Nexus1.Reporting.Application;

/// <summary>
/// Reporting had no Application layer at all before this (unlike every
/// other Phase 1/2 sector) — its entire prior existence was write-side only:
/// a message consumer that projects RootCause events into RootCauseCaseSummary
/// rows (ADR-012). This is the first read-side query added, for the BFF's
/// Trends &amp; History screen.
///
/// Named gap: this is case/investigation history (root-cause analyses opened
/// and, eventually, their verdicts), NOT a generic time-series of sensor
/// readings. Reporting's domain model has no such concept — RootCauseCaseSummary
/// is the entire domain model (plus its own two-lookup enum, ReportingCaseStatus).
/// A "Trends &amp; History" screen for a unit, honestly shaped around what this
/// context actually is, shows its investigation history, not a trend graph.
/// </summary>
public interface ICaseSummaryFinder
{
    Task<IReadOnlyList<CaseSummaryDto>> GetCaseSummariesForUnitAsync(int unitId, CancellationToken cancellationToken);
}
