namespace Nexus1.Maintenance.Application;

/// <summary>
/// Atlas C.9.5.2 query 1 projection, adapted: UnitCode, AssetCode, Name,
/// Category, Status, and the raw EquipmentId passport (not EquipmentCode) —
/// ReactorFleet.Equipment does not exist in this codebase's Phase 1 slice
/// (ReactorFleetDbContext only exposes Unit/UnitPowerSnapshot), the same
/// finding ADR-021 records for Instrumentation/DigitalTwin's own equivalent
/// corrections, so the atlas's own "LEFT JOIN ReactorFleet.Equipment ...
/// AS e.EquipmentCode" cannot be built literally. This still proves the real,
/// buildable half of the query's intent: which asset belongs to which unit,
/// category and status, plus which physical equipment passport (if any) it
/// points at.
/// </summary>
public sealed record AssetByUnitDto(string UnitCode, string AssetCode, string Name, string Category, string Status, int? EquipmentId);
