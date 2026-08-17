using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

/// <summary>RadiationReading's defining behavior (ADR-024): records a new append-only reading against a monitor.</summary>
public sealed record RecordRadiationReadingCommand(
    int RadiationMonitorId, int MeasurementTypeId, int EngineeringUnitId, int MeasurementQualityId,
    DateTime TimestampUtc, decimal Value, bool IsAlarmRelevant = false, DateTime? SourceTimestampUtc = null)
    : ICommand<long>;
