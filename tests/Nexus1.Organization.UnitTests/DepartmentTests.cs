using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class DepartmentTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var department = Department.Create(new DepartmentId(1), new LegalEntityId(1), new DepartmentTypeId(1), "OPS", "Operations", NowUtc);

        Assert.Equal("OPS", department.Code);
        Assert.Null(department.ParentDepartmentId);
    }

    [Fact]
    public void Create_with_parent_department_succeeds()
    {
        var department = Department.Create(
            new DepartmentId(2), new LegalEntityId(1), new DepartmentTypeId(1), "OPS-A", "Operations A", NowUtc,
            parentDepartmentId: new DepartmentId(1), costCentreCode: "CC-100");

        Assert.Equal(new DepartmentId(1), department.ParentDepartmentId);
        Assert.Equal("CC-100", department.CostCentreCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Department.Create(new DepartmentId(1), new LegalEntityId(1), new DepartmentTypeId(1), code, "Operations", NowUtc));
    }
}
