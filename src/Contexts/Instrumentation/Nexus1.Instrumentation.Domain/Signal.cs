using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>
/// The canonical tag (atlas C.5.4.5): digital twin, alarms, root cause and
/// historian all reference this table. Tag is the real unique business key
/// (UQ_Instrumentation_Signal_Tag), per the book's own "is the tag stable
/// and unambiguous?" design question (ADR-019).
///
/// UnitId/EquipmentId/PlantSystemId/EngineeringUnitId are plain ints, not
/// shared types from another context's Domain project — Domain never
/// references another context's Domain (dependency law). UnitId and
/// EngineeringUnitId get a real SQL FOREIGN KEY at the Infrastructure layer
/// (to ReactorFleet.Unit and CorePlatform.EngineeringUnit respectively,
/// ADR-019). EquipmentId/PlantSystemId do NOT get an enforced FK: this
/// repository's ReactorFleet slice (Phase 1, ADR-003) only ever built
/// ReactorFleet.Unit — ReactorFleet.Equipment and ReactorFleet.PlantSystem
/// do not exist in this codebase, so no real FOREIGN KEY can reference them.
/// See the session's final report for this discrepancy against ADR-019's
/// assumption that those two tables were available to FK against.
///
/// SensorChannelId is intentionally omitted: the atlas's own DDL makes it
/// nullable, and Instrument/Sensor/SensorChannel are out of scope for this
/// build (ADR-019) — nothing in this build would ever populate it.
/// </summary>
public sealed class Signal : Entity<SignalId>, IAggregateRoot
{
    private Signal(
        SignalId id, int unitId, int? equipmentId, int? plantSystemId, SignalTypeId signalTypeId,
        SignalCategoryId signalCategoryId, SignalRoleId signalRoleId, int engineeringUnitId, SamplingModeId samplingModeId,
        HistorianRetentionClassId historianRetentionClassId, string tag, string name, string? description,
        decimal? scanRateHz, int? precisionDigits, decimal? normalMin, decimal? normalMax, bool isSafetyRelated,
        bool isHistorized, DateTime createdAtUtc)
        : base(id)
    {
        UnitId = unitId;
        EquipmentId = equipmentId;
        PlantSystemId = plantSystemId;
        SignalTypeId = signalTypeId;
        SignalCategoryId = signalCategoryId;
        SignalRoleId = signalRoleId;
        EngineeringUnitId = engineeringUnitId;
        SamplingModeId = samplingModeId;
        HistorianRetentionClassId = historianRetentionClassId;
        Tag = tag;
        Name = name;
        Description = description;
        ScanRateHz = scanRateHz;
        PrecisionDigits = precisionDigits;
        NormalMin = normalMin;
        NormalMax = normalMax;
        IsSafetyRelated = isSafetyRelated;
        IsHistorized = isHistorized;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>ReactorFleet.Unit real FK (ADR-019).</summary>
    public int UnitId { get; }

    /// <summary>ReactorFleet.Equipment passport id — no enforced FK; that table does not exist in this codebase's ReactorFleet slice (Phase 1, ADR-003). See ADR-019 discrepancy note above.</summary>
    public int? EquipmentId { get; }

    /// <summary>ReactorFleet.PlantSystem passport id — no enforced FK; that table does not exist in this codebase's ReactorFleet slice (Phase 1, ADR-003). See ADR-019 discrepancy note above.</summary>
    public int? PlantSystemId { get; }

    public SignalTypeId SignalTypeId { get; }

    public SignalCategoryId SignalCategoryId { get; }

    public SignalRoleId SignalRoleId { get; }

    /// <summary>CorePlatform.EngineeringUnit real FK (ADR-019).</summary>
    public int EngineeringUnitId { get; }

    public SamplingModeId SamplingModeId { get; }

    public HistorianRetentionClassId HistorianRetentionClassId { get; }

    public string Tag { get; }

    public string Name { get; }

    public string? Description { get; }

    public decimal? ScanRateHz { get; }

    public int? PrecisionDigits { get; }

    public decimal? NormalMin { get; }

    public decimal? NormalMax { get; }

    public bool IsSafetyRelated { get; }

    public bool IsHistorized { get; }

    public DateTime CreatedAtUtc { get; }

    public static Signal Create(
        SignalId id, int unitId, SignalTypeId signalTypeId, SignalCategoryId signalCategoryId, SignalRoleId signalRoleId,
        int engineeringUnitId, SamplingModeId samplingModeId, HistorianRetentionClassId historianRetentionClassId,
        string tag, string name, DateTime createdAtUtc, int? equipmentId = null, int? plantSystemId = null,
        string? description = null, decimal? scanRateHz = null, int? precisionDigits = null, decimal? normalMin = null,
        decimal? normalMax = null, bool isSafetyRelated = false, bool isHistorized = true)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Signal tag must not be empty.", nameof(tag));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Signal name must not be empty.", nameof(name));
        }

        if (normalMin is not null && normalMax is not null && normalMax <= normalMin)
        {
            throw new ArgumentException("NormalMax must be greater than NormalMin when both are set.", nameof(normalMax));
        }

        if (scanRateHz is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scanRateHz), scanRateHz, "ScanRateHz must be greater than zero when set.");
        }

        return new Signal(
            id, unitId, equipmentId, plantSystemId, signalTypeId, signalCategoryId, signalRoleId, engineeringUnitId,
            samplingModeId, historianRetentionClassId, tag, name, description, scanRateHz, precisionDigits, normalMin,
            normalMax, isSafetyRelated, isHistorized, createdAtUtc);
    }
}
