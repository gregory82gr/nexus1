using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>
/// Subcomponent tree below a maintainable asset: seal, bearing, actuator,
/// motor winding, module (atlas C.9.2). Self-referencing via
/// ParentAssetComponentId. Not itself the subject of any C.9.5.2
/// verification query, but included in ADR-021's scope because
/// DegradationRecord.AssetComponentId (nullable) needs a real internal FK
/// target to be meaningful.
///
/// Audit columns (CreatedBy, ModifiedAtUtc, ModifiedBy, IsDeleted,
/// RowVersion) are not modeled in Domain, same restraint as every prior
/// sector — no Application operation filters on AssetComponent.IsDeleted,
/// unlike Asset/WorkOrder.
/// </summary>
public sealed class AssetComponent : Entity<AssetComponentId>, IAggregateRoot
{
    private AssetComponent(
        AssetComponentId id, AssetId assetId, AssetComponentId? parentAssetComponentId, string componentCode,
        string name, string? description, bool isReplaceable, DateTime createdAtUtc)
        : base(id)
    {
        AssetId = assetId;
        ParentAssetComponentId = parentAssetComponentId;
        ComponentCode = componentCode;
        Name = name;
        Description = description;
        IsReplaceable = isReplaceable;
        CreatedAtUtc = createdAtUtc;
    }

    public AssetId AssetId { get; }

    public AssetComponentId? ParentAssetComponentId { get; }

    public string ComponentCode { get; }

    public string Name { get; }

    public string? Description { get; }

    public bool IsReplaceable { get; }

    public DateTime CreatedAtUtc { get; }

    public static AssetComponent Create(
        AssetComponentId id, AssetId assetId, string componentCode, string name, DateTime createdAtUtc,
        AssetComponentId? parentAssetComponentId = null, string? description = null, bool isReplaceable = true)
    {
        if (string.IsNullOrWhiteSpace(componentCode))
        {
            throw new ArgumentException("AssetComponent code must not be empty.", nameof(componentCode));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("AssetComponent name must not be empty.", nameof(name));
        }

        return new AssetComponent(id, assetId, parentAssetComponentId, componentCode, name, description, isReplaceable, createdAtUtc);
    }
}
