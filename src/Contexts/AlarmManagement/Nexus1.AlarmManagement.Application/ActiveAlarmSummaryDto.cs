namespace Nexus1.AlarmManagement.Application;

/// <summary>
/// Shaped for the BFF's fleet-wide alarm-monitoring screen (ADR-030's
/// AlarmManagement slice) — unlike ActiveAlarmDto (GetActiveAlarmsForUnitQuery),
/// this crosses units, so UnitId is included; the screen already knowing
/// which unit an alarm belongs to is the whole point of a fleet-wide view.
/// </summary>
public sealed record ActiveAlarmSummaryDto(
    long AlarmEventId,
    int UnitId,
    string Message,
    string Severity,
    DateTime RaisedAtUtc);
