using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>
/// A robot platform model/catalogue entry (ADR-023). RobotTypeId is a real
/// internal FK and NOT NULL — a RobotModel cannot exist without a valid type,
/// matching the atlas's own NOT NULL chain (Robot -> RobotModel -> RobotType).
///
/// Full audit shape (CreatedAtUtc/CreatedBy/ModifiedAtUtc/ModifiedBy/IsDeleted/
/// RowVersion) is NOT modeled in Domain at all — EF shadow properties only,
/// same restraint as OperationalEventConfiguration's own treatment (ADR-023).
/// </summary>
public sealed class RobotModel : Entity<RobotModelId>, IAggregateRoot
{
    private RobotModel(
        RobotModelId id, RobotTypeId robotTypeId, string code, string manufacturer, string modelName,
        string? description, decimal? maxPayloadKg, decimal? maxSpeedMps, decimal? batteryCapacityWh,
        int? nominalRuntimeMin, bool isAutonomousCapable)
        : base(id)
    {
        RobotTypeId = robotTypeId;
        Code = code;
        Manufacturer = manufacturer;
        ModelName = modelName;
        Description = description;
        MaxPayloadKg = maxPayloadKg;
        MaxSpeedMps = maxSpeedMps;
        BatteryCapacityWh = batteryCapacityWh;
        NominalRuntimeMin = nominalRuntimeMin;
        IsAutonomousCapable = isAutonomousCapable;
    }

    public RobotTypeId RobotTypeId { get; }

    public string Code { get; }

    public string Manufacturer { get; }

    public string ModelName { get; }

    public string? Description { get; }

    public decimal? MaxPayloadKg { get; }

    public decimal? MaxSpeedMps { get; }

    public decimal? BatteryCapacityWh { get; }

    public int? NominalRuntimeMin { get; }

    public bool IsAutonomousCapable { get; }

    public static RobotModel Create(
        RobotModelId id, RobotTypeId robotTypeId, string code, string manufacturer, string modelName,
        string? description = null, decimal? maxPayloadKg = null, decimal? maxSpeedMps = null,
        decimal? batteryCapacityWh = null, int? nominalRuntimeMin = null, bool isAutonomousCapable = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RobotModel code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(manufacturer))
        {
            throw new ArgumentException("RobotModel manufacturer must not be empty.", nameof(manufacturer));
        }

        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException("RobotModel model name must not be empty.", nameof(modelName));
        }

        return new RobotModel(
            id, robotTypeId, code, manufacturer, modelName, description, maxPayloadKg, maxSpeedMps,
            batteryCapacityWh, nominalRuntimeMin, isAutonomousCapable);
    }
}
