using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

/// <summary>RadiationZone's defining behavior (ADR-024): registers a new zone against its three-lookup chain.</summary>
public sealed record RegisterRadiationZoneCommand(
    int RadiationZoneTypeId, int RadiationZoneStatusId, int RadiationAreaClassificationId, string Code, string Name,
    int? UnitId = null, int? EquipmentLocationId = null, string? Description = null, bool IsEntryControlled = true,
    bool RequiresDosimeter = true, DateTime? PostedAtUtc = null)
    : ICommand<int>;
