namespace Nexus1.ReactorFleet.Application;

/// <summary>
/// Shaped for the BFF's fleet-overview screen (ADR-030), not a 1:1 mirror of
/// Unit — only Code/Name exist on the Phase 1 Unit aggregate itself (ADR-003);
/// LatestPowerPercent/LatestPowerRecordedAtUtc come from the most recent
/// UnitPowerSnapshot and are null for a unit with no recorded snapshot yet.
/// </summary>
public sealed record UnitSummaryDto(
    int Id,
    string Code,
    string Name,
    decimal? LatestPowerPercent,
    DateTime? LatestPowerRecordedAtUtc);
