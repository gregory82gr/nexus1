namespace Nexus1.RadiationMonitoring.Application;

/// <summary>
/// One monitor sited at the unit, with its latest reading if it has one.
/// LatestValue/EngineeringUnitSymbol/Quality/LatestReadingAtUtc are all
/// nullable — a monitor can be sited before it has ever reported (mirrors
/// RadiationMonitor's own nullable-until-sited design).
/// </summary>
public sealed record UnitRadiationMonitorReadingDto(
    string MonitorCode,
    string MonitorName,
    string MonitorStatus,
    decimal? LatestValue,
    string? EngineeringUnitSymbol,
    string? Quality,
    DateTime? LatestReadingAtUtc);
