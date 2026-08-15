namespace Nexus1.AlarmManagement.Application;

public sealed record ActiveAlarmDto(long AlarmEventId, string Message, string Severity, DateTime RaisedAtUtc);
