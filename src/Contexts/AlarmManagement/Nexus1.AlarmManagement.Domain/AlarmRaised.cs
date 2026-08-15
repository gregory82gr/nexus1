namespace Nexus1.AlarmManagement.Domain;

public sealed record AlarmRaised(
    AlarmEventId AlarmEventId,
    AlarmDefinitionId AlarmDefinitionId,
    UnitId UnitId,
    AlarmSeverity Severity,
    DateTime RaisedAtUtc);
