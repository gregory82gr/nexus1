using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

public class AssetComponentTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_is_replaceable_by_default()
    {
        var component = AssetComponent.Create(new AssetComponentId(1), new AssetId(1), "SEAL-01", "Mechanical Seal", NowUtc);

        Assert.Equal("SEAL-01", component.ComponentCode);
        Assert.True(component.IsReplaceable);
        Assert.Null(component.ParentAssetComponentId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_component_code_throws(string componentCode)
    {
        Assert.Throws<ArgumentException>(() => AssetComponent.Create(new AssetComponentId(1), new AssetId(1), componentCode, "Mechanical Seal", NowUtc));
    }

    [Fact]
    public void Create_with_parent_component_sets_the_self_referencing_link()
    {
        var component = AssetComponent.Create(
            new AssetComponentId(2), new AssetId(1), "BEARING-01", "Bearing", NowUtc, parentAssetComponentId: new AssetComponentId(1));

        Assert.Equal(new AssetComponentId(1), component.ParentAssetComponentId);
    }
}
