using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void AssetCategory_Create_with_valid_fields_succeeds()
    {
        var category = AssetCategory.Create(new AssetCategoryId(1), "PUMP", "Pump", NowUtc);
        Assert.Equal("PUMP", category.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AssetCategory_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => AssetCategory.Create(new AssetCategoryId(1), code, "Pump", NowUtc));
    }

    [Fact]
    public void AssetStatus_Create_with_valid_fields_succeeds()
    {
        var status = AssetStatus.Create(new AssetStatusId(1), "IN_SERVICE", "In Service", NowUtc);
        Assert.Equal("IN_SERVICE", status.Code);
    }

    [Fact]
    public void AssetCriticality_Create_with_valid_fields_succeeds()
    {
        var criticality = AssetCriticality.Create(new AssetCriticalityId(1), "HIGH", "High", NowUtc);
        Assert.Equal("HIGH", criticality.Code);
    }

    [Fact]
    public void ConditionGrade_Create_with_valid_fields_succeeds()
    {
        var grade = ConditionGrade.Create(new ConditionGradeId(1), "A", "Excellent", NowUtc);
        Assert.Equal("A", grade.Code);
    }

    [Fact]
    public void DegradationMechanism_Create_with_valid_fields_succeeds()
    {
        var mechanism = DegradationMechanism.Create(new DegradationMechanismId(1), "CORROSION", "Corrosion", NowUtc);
        Assert.Equal("CORROSION", mechanism.Code);
    }

    [Fact]
    public void FindingSeverity_Create_with_valid_fields_succeeds()
    {
        var severity = FindingSeverity.Create(new FindingSeverityId(1), "MAJOR", "Major", NowUtc);
        Assert.Equal("MAJOR", severity.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FindingSeverity_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => FindingSeverity.Create(new FindingSeverityId(1), "MAJOR", name, NowUtc));
    }

    [Fact]
    public void WorkOrderType_Create_with_valid_fields_succeeds()
    {
        var type = WorkOrderType.Create(new WorkOrderTypeId(1), "CORRECTIVE", "Corrective", NowUtc);
        Assert.Equal("CORRECTIVE", type.Code);
    }

    [Fact]
    public void WorkOrderStatus_Create_with_valid_fields_succeeds()
    {
        var status = WorkOrderStatus.Create(new WorkOrderStatusId(1), "PLANNED", "Planned", NowUtc);
        Assert.Equal("PLANNED", status.Code);
    }

    [Fact]
    public void WorkPriority_Create_with_valid_fields_succeeds()
    {
        var priority = WorkPriority.Create(new WorkPriorityId(1), "URGENT", "Urgent", NowUtc);
        Assert.Equal("URGENT", priority.Code);
    }
}
