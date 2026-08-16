using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class PositionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var position = Position.Create(
            new PositionId(1), "REACTOR-OP", "Reactor Operator", NowUtc,
            departmentId: new DepartmentId(1), isSafetyCritical: true, requiresShiftWork: true);

        Assert.True(position.IsSafetyCritical);
        Assert.True(position.RequiresShiftWork);
        Assert.Equal(new DepartmentId(1), position.DepartmentId);
    }

    [Fact]
    public void Create_without_department_succeeds()
    {
        var position = Position.Create(new PositionId(1), "REACTOR-OP", "Reactor Operator", NowUtc);
        Assert.Null(position.DepartmentId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<ArgumentException>(() => Position.Create(new PositionId(1), "REACTOR-OP", title, NowUtc));
    }
}
