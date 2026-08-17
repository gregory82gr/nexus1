namespace Nexus1.Maintenance.Application;

/// <summary>Atlas C.9.5.2 query 5 projection, verbatim: AssetCode, Mechanism, Severity, DetectedAtUtc, TrendPoints (count of DegradationTrendPoint rows).</summary>
public sealed record ActiveDegradationCaseDto(string AssetCode, string Mechanism, string Severity, DateTime DetectedAtUtc, int TrendPoints);
