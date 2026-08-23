namespace Nexus1.Reporting.Application;

/// <summary>
/// Shaped for the BFF's Trends &amp; History screen. Reporting's real domain
/// model is a root-cause investigation case history per unit (cases opened
/// from an alarm flood, eventually closed with a verdict) — not raw sensor
/// trend data over time. See ICaseSummaryFinder's doc comment for the full
/// explanation of why the endpoint is shaped this way.
/// </summary>
public sealed record CaseSummaryDto(
    long CaseId,
    int UnitId,
    long AlarmFloodId,
    string Status,
    string? Verdict,
    DateTime OpenedAtUtc,
    DateTime? VerdictIssuedAtUtc);
