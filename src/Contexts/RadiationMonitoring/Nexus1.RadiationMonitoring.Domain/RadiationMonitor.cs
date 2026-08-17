using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// A physical radiation-monitoring instrument (ADR-024). Nullable on all
/// three of its plant-topology anchors (UnitId/EquipmentId/RadiationZoneId)
/// — a monitor can exist before it is sited, matching the atlas's own DDL
/// exactly.
///
/// UnitId is a real SQL FOREIGN KEY to ReactorFleet.Unit via the
/// ReactorFleetUnitReference shadow-entity technique. EquipmentId is
/// deliberately downgraded to a plain nullable passport int, no enforced FK
/// — its target table (ReactorFleet.Equipment) does not exist in this
/// codebase. RadiationZoneId is a real internal FK, nullable, typed to
/// match RadiationZone's own strongly-typed Id.
///
/// MonitorTypeId/MonitorStatusId are real internal FKs and NOT NULL.
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only, same treatment as RadiationZone (ADR-024).
/// </summary>
public sealed class RadiationMonitor : Entity<RadiationMonitorId>, IAggregateRoot
{
    private RadiationMonitor(
        RadiationMonitorId id, int? unitId, int? equipmentId, RadiationZoneId? radiationZoneId,
        MonitorTypeId monitorTypeId, MonitorStatusId monitorStatusId, string code, string name,
        string? serialNumber, DateTime? installedAtUtc, DateTime? calibrationDueAtUtc)
        : base(id)
    {
        UnitId = unitId;
        EquipmentId = equipmentId;
        RadiationZoneId = radiationZoneId;
        MonitorTypeId = monitorTypeId;
        MonitorStatusId = monitorStatusId;
        Code = code;
        Name = name;
        SerialNumber = serialNumber;
        InstalledAtUtc = installedAtUtc;
        CalibrationDueAtUtc = calibrationDueAtUtc;
    }

    /// <summary>Real FK to ReactorFleet.Unit (ADR-024), nullable — a monitor can exist before it is sited.</summary>
    public int? UnitId { get; }

    /// <summary>Passport-only — ReactorFleet.Equipment does not exist in this codebase (ADR-024).</summary>
    public int? EquipmentId { get; }

    /// <summary>Real internal FK to RadiationZone, nullable — a monitor can exist before it is sited.</summary>
    public RadiationZoneId? RadiationZoneId { get; }

    public MonitorTypeId MonitorTypeId { get; }

    public MonitorStatusId MonitorStatusId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? SerialNumber { get; }

    public DateTime? InstalledAtUtc { get; }

    public DateTime? CalibrationDueAtUtc { get; }

    public static RadiationMonitor Create(
        RadiationMonitorId id, MonitorTypeId monitorTypeId, MonitorStatusId monitorStatusId, string code,
        string name, int? unitId = null, int? equipmentId = null, RadiationZoneId? radiationZoneId = null,
        string? serialNumber = null, DateTime? installedAtUtc = null, DateTime? calibrationDueAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RadiationMonitor code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("RadiationMonitor name must not be empty.", nameof(name));
        }

        return new RadiationMonitor(
            id, unitId, equipmentId, radiationZoneId, monitorTypeId, monitorStatusId, code, name, serialNumber,
            installedAtUtc, calibrationDueAtUtc);
    }
}
