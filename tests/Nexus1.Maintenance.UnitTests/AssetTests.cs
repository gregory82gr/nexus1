using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

public class AssetTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_is_not_deleted()
    {
        var asset = Asset.Create(
            new AssetId(1), unitId: 1, new AssetCategoryId(1), new AssetStatusId(1), new AssetCriticalityId(1),
            "NX1-U1-PMP-001", "Feedwater Pump 1", NowUtc);

        Assert.Equal("NX1-U1-PMP-001", asset.AssetCode);
        Assert.False(asset.IsDeleted);
        Assert.False(asset.IsSafetyRelated);
        Assert.Null(asset.EquipmentId);
        Assert.Null(asset.SystemId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_asset_code_throws(string assetCode)
    {
        Assert.Throws<ArgumentException>(() => Asset.Create(
            new AssetId(1), 1, new AssetCategoryId(1), new AssetStatusId(1), new AssetCriticalityId(1),
            assetCode, "Feedwater Pump 1", NowUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Asset.Create(
            new AssetId(1), 1, new AssetCategoryId(1), new AssetStatusId(1), new AssetCriticalityId(1),
            "NX1-U1-PMP-001", name, NowUtc));
    }

    [Fact]
    public void Create_accepts_passport_only_equipment_and_system_ids()
    {
        var asset = Asset.Create(
            new AssetId(1), unitId: 1, new AssetCategoryId(1), new AssetStatusId(1), new AssetCriticalityId(1),
            "NX1-U1-PMP-001", "Feedwater Pump 1", NowUtc, equipmentId: 42, systemId: 7);

        Assert.Equal(42, asset.EquipmentId);
        Assert.Equal(7, asset.SystemId);
    }
}
