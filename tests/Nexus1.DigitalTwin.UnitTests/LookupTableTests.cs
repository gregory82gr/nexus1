using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TwinModelType_Create_with_valid_fields_succeeds()
    {
        var type = TwinModelType.Create(new TwinModelTypeId(1), "POINT_KINETICS", "Point Kinetics", NowUtc);
        Assert.Equal("POINT_KINETICS", type.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TwinModelType_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => TwinModelType.Create(new TwinModelTypeId(1), code, "Point Kinetics", NowUtc));
    }

    [Fact]
    public void TwinModelStatus_Create_with_valid_fields_succeeds()
    {
        var status = TwinModelStatus.Create(new TwinModelStatusId(1), "ACTIVE", "Active", NowUtc);
        Assert.Equal("ACTIVE", status.Code);
    }

    [Fact]
    public void TwinFidelityLevel_Create_with_valid_fields_succeeds()
    {
        var level = TwinFidelityLevel.Create(new TwinFidelityLevelId(1), "VALIDATED", "Validated", NowUtc);
        Assert.Equal("VALIDATED", level.Code);
    }

    [Fact]
    public void ModelVariableType_Create_with_valid_fields_succeeds()
    {
        var type = ModelVariableType.Create(new ModelVariableTypeId(1), "STATE", "State", NowUtc);
        Assert.Equal("STATE", type.Code);
    }

    [Fact]
    public void SolverType_Create_with_valid_fields_succeeds()
    {
        var type = SolverType.Create(new SolverTypeId(1), "RK4", "Runge-Kutta 4", NowUtc);
        Assert.Equal("RK4", type.Code);
    }

    [Fact]
    public void ValidationStatus_Create_with_valid_fields_succeeds()
    {
        var status = ValidationStatus.Create(new ValidationStatusId(1), "PASSED", "Passed", NowUtc);
        Assert.Equal("PASSED", status.Code);
    }

    [Fact]
    public void BindingRole_Create_with_valid_fields_succeeds()
    {
        var role = BindingRole.Create(new BindingRoleId(1), "INPUT", "Input", NowUtc);
        Assert.Equal("INPUT", role.Code);
    }

    [Fact]
    public void BindingStatus_Create_with_valid_fields_succeeds()
    {
        var status = BindingStatus.Create(new BindingStatusId(1), "ACTIVE", "Active", NowUtc);
        Assert.Equal("ACTIVE", status.Code);
    }

    [Fact]
    public void SnapshotReason_Create_with_valid_fields_succeeds()
    {
        var reason = SnapshotReason.Create(new SnapshotReasonId(1), "SCHEDULED", "Scheduled", NowUtc);
        Assert.Equal("SCHEDULED", reason.Code);
    }

    [Fact]
    public void DivergenceSeverity_Create_with_valid_fields_succeeds()
    {
        var severity = DivergenceSeverity.Create(new DivergenceSeverityId(1), "WARNING", "Warning", NowUtc);
        Assert.Equal("WARNING", severity.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DivergenceSeverity_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => DivergenceSeverity.Create(new DivergenceSeverityId(1), "WARNING", name, NowUtc));
    }

    [Fact]
    public void DivergenceStatus_Create_with_valid_fields_succeeds()
    {
        var status = DivergenceStatus.Create(new DivergenceStatusId(1), "OPEN", "Open", NowUtc);
        Assert.Equal("OPEN", status.Code);
    }
}
