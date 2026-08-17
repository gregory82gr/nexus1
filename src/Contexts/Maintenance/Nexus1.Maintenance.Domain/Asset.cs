using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>
/// The maintainable view of a physical component or equipment item (atlas
/// C.9.1's own anchor table, C.9.5.2 query 1's subject): "Physical identity
/// stays in ReactorFleet. Maintenance adds maintainability, work execution,
/// material planning and condition history."
///
/// UnitId is a real SQL FOREIGN KEY at the Infrastructure layer to
/// ReactorFleet.Unit via the ReactorFleetUnitReference shadow-entity
/// technique (ADR-021, following ADR-019/ADR-020's precedent).
///
/// EquipmentId and SystemId are plain passport-only nullable ints, NOT real
/// FKs — ReactorFleet.Equipment and ReactorFleet.System do not exist in this
/// codebase's Phase 1 slice (ReactorFleetDbContext only exposes Unit and
/// UnitPowerSnapshot), the same finding and treatment Instrumentation's own
/// ADR-019 correction already established. The atlas calls the second table
/// "ReactorFleet.System" in this sector; it is the same absent table
/// Instrumentation/DigitalTwin call "PlantSystem" — an atlas naming
/// inconsistency, not a different table.
///
/// AssetCriticalityId is NOT NULL per the real DDL (easy to miss from the
/// abbreviated table-list FK summary, which omits it) — enforced here simply
/// by the non-nullable AssetCriticalityId parameter type.
///
/// Audit columns (CreatedBy, ModifiedAtUtc, ModifiedBy, RowVersion) are not
/// modeled in Domain, same restraint as every prior sector. IsDeleted IS
/// modeled here despite that restraint: C.9.5.2 query 1 (GetAssetsByUnitQuery)
/// filters on "IsDeleted = 0" as its own defining behavior, so this is real
/// business data the Application layer needs, not pure bookkeeping.
/// </summary>
public sealed class Asset : Entity<AssetId>, IAggregateRoot
{
    private Asset(
        AssetId id, int unitId, int? equipmentId, int? systemId, AssetCategoryId assetCategoryId,
        AssetStatusId assetStatusId, AssetCriticalityId assetCriticalityId, string assetCode, string name,
        string? description, string? locationText, string? manufacturer, string? modelNumber, string? serialNumber,
        DateTime? commissionedAtUtc, DateTime? retiredAtUtc, bool isSafetyRelated, bool isDeleted, DateTime createdAtUtc)
        : base(id)
    {
        UnitId = unitId;
        EquipmentId = equipmentId;
        SystemId = systemId;
        AssetCategoryId = assetCategoryId;
        AssetStatusId = assetStatusId;
        AssetCriticalityId = assetCriticalityId;
        AssetCode = assetCode;
        Name = name;
        Description = description;
        LocationText = locationText;
        Manufacturer = manufacturer;
        ModelNumber = modelNumber;
        SerialNumber = serialNumber;
        CommissionedAtUtc = commissionedAtUtc;
        RetiredAtUtc = retiredAtUtc;
        IsSafetyRelated = isSafetyRelated;
        IsDeleted = isDeleted;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>ReactorFleet.Unit real FK (ADR-021) — every maintainable asset belongs to a physical unit.</summary>
    public int UnitId { get; }

    /// <summary>Passport-only, not a real FK — ReactorFleet.Equipment does not exist in this codebase (ADR-021).</summary>
    public int? EquipmentId { get; }

    /// <summary>Passport-only, not a real FK — ReactorFleet.System (aka PlantSystem elsewhere) does not exist in this codebase (ADR-021).</summary>
    public int? SystemId { get; }

    public AssetCategoryId AssetCategoryId { get; }

    public AssetStatusId AssetStatusId { get; }

    public AssetCriticalityId AssetCriticalityId { get; }

    public string AssetCode { get; }

    public string Name { get; }

    public string? Description { get; }

    public string? LocationText { get; }

    public string? Manufacturer { get; }

    public string? ModelNumber { get; }

    public string? SerialNumber { get; }

    public DateTime? CommissionedAtUtc { get; }

    public DateTime? RetiredAtUtc { get; }

    public bool IsSafetyRelated { get; }

    public bool IsDeleted { get; }

    public DateTime CreatedAtUtc { get; }

    public static Asset Create(
        AssetId id, int unitId, AssetCategoryId assetCategoryId, AssetStatusId assetStatusId,
        AssetCriticalityId assetCriticalityId, string assetCode, string name, DateTime createdAtUtc,
        int? equipmentId = null, int? systemId = null, string? description = null, string? locationText = null,
        string? manufacturer = null, string? modelNumber = null, string? serialNumber = null,
        DateTime? commissionedAtUtc = null, DateTime? retiredAtUtc = null, bool isSafetyRelated = false)
    {
        if (string.IsNullOrWhiteSpace(assetCode))
        {
            throw new ArgumentException("Asset code must not be empty.", nameof(assetCode));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Asset name must not be empty.", nameof(name));
        }

        return new Asset(
            id, unitId, equipmentId, systemId, assetCategoryId, assetStatusId, assetCriticalityId, assetCode, name,
            description, locationText, manufacturer, modelNumber, serialNumber, commissionedAtUtc, retiredAtUtc,
            isSafetyRelated, isDeleted: false, createdAtUtc);
    }
}
