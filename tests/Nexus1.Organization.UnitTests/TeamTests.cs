using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class TeamTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var team = Team.Create(new TeamId(1), new DepartmentId(1), new TeamTypeId(1), "CREW-A", "Crew A", NowUtc, isShiftTeam: true);

        Assert.True(team.IsShiftTeam);
        Assert.False(team.IsEmergencyTeam);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Team.Create(new TeamId(1), new DepartmentId(1), new TeamTypeId(1), "CREW-A", name, NowUtc));
    }
}
