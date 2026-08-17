using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// An append-only fact record of a radiation measurement (ADR-024, atlas
/// query 3's own subject) — mirrors Robotics.RobotHealthSnapshot's own
/// append-only shape. RadiationMonitorId, MeasurementTypeId and
/// MeasurementQualityId are all real internal FKs and NOT NULL.
/// EngineeringUnitId is a real SQL FOREIGN KEY to
/// CorePlatform.EngineeringUnit via the CorePlatformEngineeringUnitReference
/// shadow-entity technique and NOT NULL. No audit columns at all —
/// append-only, all fields modeled directly in Domain.
/// </summary>
public sealed class RadiationReading : Entity<RadiationReadingId>, IAggregateRoot
{
    private RadiationReading(
        RadiationReadingId id, RadiationMonitorId radiationMonitorId, MeasurementTypeId measurementTypeId,
        int engineeringUnitId, MeasurementQualityId measurementQualityId, DateTime timestampUtc, decimal value,
        bool isAlarmRelevant, DateTime? sourceTimestampUtc)
        : base(id)
    {
        RadiationMonitorId = radiationMonitorId;
        MeasurementTypeId = measurementTypeId;
        EngineeringUnitId = engineeringUnitId;
        MeasurementQualityId = measurementQualityId;
        TimestampUtc = timestampUtc;
        Value = value;
        IsAlarmRelevant = isAlarmRelevant;
        SourceTimestampUtc = sourceTimestampUtc;
    }

    public RadiationMonitorId RadiationMonitorId { get; }

    public MeasurementTypeId MeasurementTypeId { get; }

    /// <summary>Real FK to CorePlatform.EngineeringUnit (ADR-024), NOT NULL.</summary>
    public int EngineeringUnitId { get; }

    public MeasurementQualityId MeasurementQualityId { get; }

    public DateTime TimestampUtc { get; }

    public decimal Value { get; }

    public bool IsAlarmRelevant { get; }

    public DateTime? SourceTimestampUtc { get; }

    public static RadiationReading Create(
        RadiationReadingId id, RadiationMonitorId radiationMonitorId, MeasurementTypeId measurementTypeId,
        int engineeringUnitId, MeasurementQualityId measurementQualityId, DateTime timestampUtc, decimal value,
        bool isAlarmRelevant = false, DateTime? sourceTimestampUtc = null)
    {
        return new RadiationReading(
            id, radiationMonitorId, measurementTypeId, engineeringUnitId, measurementQualityId, timestampUtc,
            value, isAlarmRelevant, sourceTimestampUtc);
    }
}
