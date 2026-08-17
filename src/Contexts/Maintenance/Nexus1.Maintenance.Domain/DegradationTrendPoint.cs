using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>
/// Trend points for degradation rate, thickness loss, vibration growth or
/// other condition metric (atlas C.9.2, part of C.9.5.2 query 5's trend-point
/// count). Built as a pair with DegradationRecord, mirroring
/// AssetCondition/AssetConditionMeasurement's own pairing pattern
/// (ADR-021).
///
/// EngineeringUnitId is a real, NOT NULL SQL FOREIGN KEY to
/// CorePlatform.EngineeringUnit; SourceSignalId is a real, nullable SQL
/// FOREIGN KEY to Instrumentation.Signal — both via the ExcludeFromMigrations
/// shadow-entity technique (ADR-021, following ADR-019/ADR-020's
/// precedent).
///
/// No audit columns at all per the atlas DDL — verified directly.
/// </summary>
public sealed class DegradationTrendPoint : Entity<DegradationTrendPointId>, IAggregateRoot
{
    private DegradationTrendPoint(
        DegradationTrendPointId id, DegradationRecordId degradationRecordId, int engineeringUnitId,
        int? sourceSignalId, DateTime measuredAtUtc, double value, string? note)
        : base(id)
    {
        DegradationRecordId = degradationRecordId;
        EngineeringUnitId = engineeringUnitId;
        SourceSignalId = sourceSignalId;
        MeasuredAtUtc = measuredAtUtc;
        Value = value;
        Note = note;
    }

    public DegradationRecordId DegradationRecordId { get; }

    /// <summary>CorePlatform.EngineeringUnit real FK, NOT NULL (ADR-021).</summary>
    public int EngineeringUnitId { get; }

    /// <summary>Instrumentation.Signal real FK, nullable (ADR-021).</summary>
    public int? SourceSignalId { get; }

    public DateTime MeasuredAtUtc { get; }

    public double Value { get; }

    public string? Note { get; }

    public static DegradationTrendPoint Create(
        DegradationTrendPointId id, DegradationRecordId degradationRecordId, int engineeringUnitId,
        DateTime measuredAtUtc, double value, int? sourceSignalId = null, string? note = null)
    {
        return new DegradationTrendPoint(id, degradationRecordId, engineeringUnitId, sourceSignalId, measuredAtUtc, value, note);
    }
}
