using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// Column shape matches the Schema Atlas's real AlarmEvent table, not the
/// book's simplified 6-column illustration (ADR-004).
/// </summary>
public sealed class AlarmEvent : Entity<AlarmEventId>, IAggregateRoot
{
    private AlarmEvent(
        AlarmEventId id, AlarmDefinitionId alarmDefinitionId, UnitId unitId, AlarmSeverity severity,
        DateTime raisedAtUtc, decimal? sourceValue, decimal? thresholdValue, string message)
        : base(id)
    {
        AlarmDefinitionId = alarmDefinitionId;
        UnitId = unitId;
        Severity = severity;
        State = AlarmState.Active;
        RaisedAtUtc = raisedAtUtc;
        SourceValue = sourceValue;
        ThresholdValue = thresholdValue;
        Message = message;
    }

    public AlarmDefinitionId AlarmDefinitionId { get; }

    public UnitId UnitId { get; }

    public AlarmSeverity Severity { get; }

    public AlarmState State { get; private set; }

    public DateTime RaisedAtUtc { get; }

    public decimal? SourceValue { get; }

    public decimal? ThresholdValue { get; }

    public string Message { get; }

    public UserId? AcknowledgedBy { get; private set; }

    public DateTime? AcknowledgedAtUtc { get; private set; }

    public static AlarmEvent Raise(
        AlarmEventId id, AlarmDefinitionId alarmDefinitionId, UnitId unitId, AlarmSeverity severity,
        DateTime raisedAtUtc, decimal? sourceValue, decimal? thresholdValue, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Alarm event message must not be empty.", nameof(message));
        }

        var alarmEvent = new AlarmEvent(id, alarmDefinitionId, unitId, severity, raisedAtUtc, sourceValue, thresholdValue, message);
        alarmEvent.AddDomainEvent(new AlarmRaised(id, alarmDefinitionId, unitId, severity, raisedAtUtc));
        return alarmEvent;
    }

    public void Acknowledge(UserId userId, DateTime atUtc)
    {
        if (State != AlarmState.Active)
        {
            throw new InvalidOperationException("Only an active alarm can be acknowledged.");
        }

        State = AlarmState.Acknowledged;
        AcknowledgedBy = userId;
        AcknowledgedAtUtc = atUtc;
        AddDomainEvent(new AlarmAcknowledged(Id, userId, atUtc));
    }
}
