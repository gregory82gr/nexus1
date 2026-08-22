using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class LegalEntityTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var entity = LegalEntity.Create(new LegalEntityId(1), new LegalEntityTypeId(1), "NEXUS1-OP", "Nexus1 Operator", NowUtc);

        Assert.Equal("NEXUS1-OP", entity.Code);
        Assert.False(entity.IsOperator);
        Assert.Null(entity.ParentLegalEntityId);
        Assert.Null(entity.CountryId);
    }

    [Fact]
    public void Create_with_parent_and_passport_country_id_succeeds()
    {
        var entity = LegalEntity.Create(
            new LegalEntityId(2), new LegalEntityTypeId(1), "VENDOR-1", "Vendor One", NowUtc,
            parentLegalEntityId: new LegalEntityId(1), countryId: 42, isVendor: true);

        Assert.Equal(new LegalEntityId(1), entity.ParentLegalEntityId);
        Assert.Equal(42, entity.CountryId);
        Assert.True(entity.IsVendor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => LegalEntity.Create(new LegalEntityId(1), new LegalEntityTypeId(1), code, "Name", NowUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => LegalEntity.Create(new LegalEntityId(1), new LegalEntityTypeId(1), "CODE", name, NowUtc));
    }
}
