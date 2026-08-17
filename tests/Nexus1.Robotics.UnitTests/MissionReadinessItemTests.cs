using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class MissionReadinessItemTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_defaults_is_blocking_true()
    {
        var item = MissionReadinessItem.Create(
            new MissionReadinessItemId(1), missionReadinessAssessmentId: 1, new ReadinessStatusId(1), "Battery check");

        Assert.Equal(new MissionReadinessAssessmentId(1), item.MissionReadinessAssessmentId);
        Assert.Equal("Battery check", item.CheckName);
        Assert.True(item.IsBlocking);
        Assert.Null(item.Detail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_check_name_throws(string checkName)
    {
        Assert.Throws<ArgumentException>(() => MissionReadinessItem.Create(
            new MissionReadinessItemId(1), 1, new ReadinessStatusId(1), checkName));
    }

    [Fact]
    public void Create_with_is_blocking_false_sets_it()
    {
        var item = MissionReadinessItem.Create(
            new MissionReadinessItemId(1), 1, new ReadinessStatusId(1), "Battery check",
            detail: "Advisory only", isBlocking: false);

        Assert.False(item.IsBlocking);
        Assert.Equal("Advisory only", item.Detail);
    }
}
