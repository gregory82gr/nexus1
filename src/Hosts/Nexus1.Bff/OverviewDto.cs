using Nexus1.AlarmManagement.Application;
using Nexus1.Instrumentation.Application;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.ReactorFleet.Application;

/// <summary>
/// Plant Overview / Dashboard screen (ADR-030 follow-up) — the first
/// cross-context BFF composition. Each section reuses the query/handler
/// already built for its own context's slice, unchanged; this file adds
/// zero new Application-layer code to any context. Sections are
/// independently nullable: a section is null only when its own query
/// failed (see <see cref="Errors"/>) — a query that succeeded with "no
/// data" (e.g. no active alarms) still returns a real, non-null empty
/// collection, never null standing in for "nothing to show."
/// </summary>
public sealed record OverviewDto(
    int UnitId,
    UnitDetailDto? Unit,
    IReadOnlyList<ActiveAlarmDto>? ActiveAlarms,
    UnitRadiationSafetyDto? Radiation,
    IReadOnlyList<UnitSignalReadingDto>? Signals,
    IReadOnlyDictionary<string, string> Errors);
