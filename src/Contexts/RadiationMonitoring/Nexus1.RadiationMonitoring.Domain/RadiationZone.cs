using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// A designated radiological zone (ADR-024). Carries its own three-lookup
/// NOT NULL chain (RadiationZoneTypeId/RadiationZoneStatusId/
/// RadiationAreaClassificationId) as a real internal invariant — a zone
/// cannot exist without a type, status, and classification.
///
/// UnitId is a real SQL FOREIGN KEY to ReactorFleet.Unit via the
/// ReactorFleetUnitReference shadow-entity technique, but nullable — not
/// every zone is anchored to a specific unit.
///
/// EquipmentLocationId is deliberately downgraded to a plain nullable
/// passport int, no enforced FK — its target table
/// (ReactorFleet.EquipmentLocation) does not exist in this codebase
/// (ADR-024).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only, same treatment as Robotics.RobotModel/Robot (ADR-023/ADR-024).
/// </summary>
public sealed class RadiationZone : Entity<RadiationZoneId>, IAggregateRoot
{
    private RadiationZone(
        RadiationZoneId id, int? unitId, int? equipmentLocationId, RadiationZoneTypeId radiationZoneTypeId,
        RadiationZoneStatusId radiationZoneStatusId, RadiationAreaClassificationId radiationAreaClassificationId,
        string code, string name, string? description, bool isEntryControlled, bool requiresDosimeter,
        DateTime? postedAtUtc)
        : base(id)
    {
        UnitId = unitId;
        EquipmentLocationId = equipmentLocationId;
        RadiationZoneTypeId = radiationZoneTypeId;
        RadiationZoneStatusId = radiationZoneStatusId;
        RadiationAreaClassificationId = radiationAreaClassificationId;
        Code = code;
        Name = name;
        Description = description;
        IsEntryControlled = isEntryControlled;
        RequiresDosimeter = requiresDosimeter;
        PostedAtUtc = postedAtUtc;
    }

    /// <summary>Real FK to ReactorFleet.Unit (ADR-024), nullable — not every zone is anchored to a specific unit.</summary>
    public int? UnitId { get; }

    /// <summary>Passport-only — ReactorFleet.EquipmentLocation does not exist in this codebase (ADR-024).</summary>
    public int? EquipmentLocationId { get; }

    public RadiationZoneTypeId RadiationZoneTypeId { get; }

    public RadiationZoneStatusId RadiationZoneStatusId { get; }

    public RadiationAreaClassificationId RadiationAreaClassificationId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public bool IsEntryControlled { get; }

    public bool RequiresDosimeter { get; }

    public DateTime? PostedAtUtc { get; }

    public static RadiationZone Create(
        RadiationZoneId id, RadiationZoneTypeId radiationZoneTypeId, RadiationZoneStatusId radiationZoneStatusId,
        RadiationAreaClassificationId radiationAreaClassificationId, string code, string name,
        int? unitId = null, int? equipmentLocationId = null, string? description = null,
        bool isEntryControlled = true, bool requiresDosimeter = true, DateTime? postedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RadiationZone code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("RadiationZone name must not be empty.", nameof(name));
        }

        return new RadiationZone(
            id, unitId, equipmentLocationId, radiationZoneTypeId, radiationZoneStatusId,
            radiationAreaClassificationId, code, name, description, isEntryControlled, requiresDosimeter,
            postedAtUtc);
    }
}
