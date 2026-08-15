using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// Phase 1 slice only (ADR-004): a single-threshold condition. The Schema
/// Atlas's AlarmCondition/AlarmLimit/AlarmDeadband tree (multiple condition
/// types, hysteresis, on/off delay) is deliberately not modeled — nothing in
/// Phase 1 needs more than "does this value cross this one limit."
/// </summary>
public sealed class AlarmDefinition : Entity<AlarmDefinitionId>, IAggregateRoot
{
    private AlarmDefinition(AlarmDefinitionId id, UnitId unitId, string code, string name, AlarmSeverity severity, decimal thresholdValue)
        : base(id)
    {
        UnitId = unitId;
        Code = code;
        Name = name;
        Severity = severity;
        ThresholdValue = thresholdValue;
    }

    public UnitId UnitId { get; }

    public string Code { get; }

    public string Name { get; }

    public AlarmSeverity Severity { get; }

    public decimal ThresholdValue { get; }

    public static AlarmDefinition Create(
        AlarmDefinitionId id, UnitId unitId, string code, string name, AlarmSeverity severity, decimal thresholdValue)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Alarm definition code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Alarm definition name must not be empty.", nameof(name));
        }

        return new AlarmDefinition(id, unitId, code, name, severity, thresholdValue);
    }

    /// <summary>Returns a raised AlarmEvent if sourceValue crosses ThresholdValue, else null.</summary>
    public AlarmEvent? Evaluate(decimal sourceValue, AlarmEventId newEventId, DateTime atUtc)
    {
        if (sourceValue < ThresholdValue)
        {
            return null;
        }

        return AlarmEvent.Raise(
            newEventId, Id, UnitId, Severity, atUtc, sourceValue, ThresholdValue,
            $"{Code} breached: {sourceValue} >= {ThresholdValue}.");
    }
}
