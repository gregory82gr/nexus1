namespace Nexus1.ReactorFleet.Application;

/// <summary>
/// Shaped for the BFF's unit-detail screen (ADR-030): the summary fields plus
/// recent power history, the natural thing a detail view wants that a fleet
/// overview row doesn't. RecentPowerSnapshots is ordered most-recent-first,
/// capped at 10 (see EfUnitFleetFinder) — a fixed screen need, not a paged feed.
/// </summary>
public sealed record UnitDetailDto(
    int Id,
    string Code,
    string Name,
    decimal? LatestPowerPercent,
    DateTime? LatestPowerRecordedAtUtc,
    IReadOnlyList<UnitPowerSnapshotDto> RecentPowerSnapshots);
