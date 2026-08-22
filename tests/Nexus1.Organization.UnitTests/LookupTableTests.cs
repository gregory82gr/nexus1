using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void LegalEntityType_Create_with_valid_fields_succeeds()
    {
        var type = LegalEntityType.Create(new LegalEntityTypeId(1), "OPERATOR", "Operator", NowUtc);
        Assert.Equal("OPERATOR", type.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LegalEntityType_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => LegalEntityType.Create(new LegalEntityTypeId(1), code, "Operator", NowUtc));
    }

    [Fact]
    public void SiteType_Create_with_valid_fields_succeeds()
    {
        var type = SiteType.Create(new SiteTypeId(1), "PLANT_SITE", "Plant Site", NowUtc);
        Assert.Equal("PLANT_SITE", type.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SiteType_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => SiteType.Create(new SiteTypeId(1), "PLANT_SITE", name, NowUtc));
    }

    [Fact]
    public void PlantType_Create_with_valid_fields_succeeds()
    {
        var type = PlantType.Create(new PlantTypeId(1), "NUCLEAR_DEMO", "Nuclear Demo", NowUtc);
        Assert.Equal("NUCLEAR_DEMO", type.Code);
    }

    [Fact]
    public void DepartmentType_Create_with_valid_fields_succeeds()
    {
        var type = DepartmentType.Create(new DepartmentTypeId(1), "OPS", "Operations", NowUtc);
        Assert.Equal("OPS", type.Code);
    }

    [Fact]
    public void TeamType_Create_with_valid_fields_succeeds()
    {
        var type = TeamType.Create(new TeamTypeId(1), "SHIFT_CREW", "Shift Crew", NowUtc);
        Assert.Equal("SHIFT_CREW", type.Code);
    }

    [Fact]
    public void PersonType_Create_with_valid_fields_succeeds()
    {
        var type = PersonType.Create(new PersonTypeId(1), "EMPLOYEE", "Employee", NowUtc);
        Assert.Equal("EMPLOYEE", type.Code);
    }

    [Fact]
    public void EmploymentStatus_Create_with_valid_fields_succeeds()
    {
        var status = EmploymentStatus.Create(new EmploymentStatusId(1), "ACTIVE", "Active", NowUtc);
        Assert.Equal("ACTIVE", status.Code);
    }

    [Fact]
    public void QualificationStatus_Create_with_valid_fields_succeeds()
    {
        var status = QualificationStatus.Create(new QualificationStatusId(1), "VALID", "Valid", NowUtc);
        Assert.Equal("VALID", status.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void QualificationStatus_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => QualificationStatus.Create(new QualificationStatusId(1), code, "Valid", NowUtc));
    }
}
