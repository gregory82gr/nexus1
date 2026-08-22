using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>
/// Measurement evidence used by a condition assessment (atlas C.9.2). Built
/// as a pair with AssetCondition deliberately, mirroring DigitalTwin's own
/// TwinSnapshot/TwinSnapshotValue pairing (ADR-020 precedent, cited by
/// ADR-021): an AssetCondition alone (a health score and RUL estimate) would
/// be an unsupported opinion — this is what ties an assessment to real
/// measured evidence.
///
/// SignalId is a real, nullable SQL FOREIGN KEY at the Infrastructure layer
/// to Instrumentation.Signal via the InstrumentationSignalReference
/// shadow-entity technique. EngineeringUnitId is a real, NOT NULL SQL
/// FOREIGN KEY to CorePlatform.EngineeringUnit via the
/// CorePlatformEngineeringUnitReference shadow-entity technique (both
/// ADR-021, following ADR-019/ADR-020's precedent).
///
/// No audit columns at all per the atlas DDL — verified directly, this is
/// the leanest table in the sector.
/// </summary>
public sealed class AssetConditionMeasurement : Entity<AssetConditionMeasurementId>, IAggregateRoot
{
    private AssetConditionMeasurement(
        AssetConditionMeasurementId id, AssetConditionId assetConditionId, int? signalId, int engineeringUnitId,
        double measuredValue, DateTime measuredAtUtc, string? measurementNote)
        : base(id)
    {
        AssetConditionId = assetConditionId;
        SignalId = signalId;
        EngineeringUnitId = engineeringUnitId;
        MeasuredValue = measuredValue;
        MeasuredAtUtc = measuredAtUtc;
        MeasurementNote = measurementNote;
    }

    public AssetConditionId AssetConditionId { get; }

    /// <summary>Instrumentation.Signal real FK, nullable (ADR-021).</summary>
    public int? SignalId { get; }

    /// <summary>CorePlatform.EngineeringUnit real FK, NOT NULL (ADR-021).</summary>
    public int EngineeringUnitId { get; }

    public double MeasuredValue { get; }

    public DateTime MeasuredAtUtc { get; }

    public string? MeasurementNote { get; }

    public static AssetConditionMeasurement Create(
        AssetConditionMeasurementId id, AssetConditionId assetConditionId, int engineeringUnitId, double measuredValue,
        DateTime measuredAtUtc, int? signalId = null, string? measurementNote = null)
    {
        return new AssetConditionMeasurement(
            id, assetConditionId, signalId, engineeringUnitId, measuredValue, measuredAtUtc, measurementNote);
    }
}
