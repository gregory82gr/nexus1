using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class MissionEventTests
{
    private static readonly DateTime OccurredAtUtc = new(2026, 8, 17, 9, 5, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_defaults_is_fault_false()
    {
        var missionEvent = MissionEvent.Create(
            new MissionEventId(1), missionId: 1, robotId: new RobotId(1), OccurredAtUtc, "DISPATCHED", "Mission dispatched");

        Assert.Equal(new MissionId(1), missionEvent.MissionId);
        Assert.Equal(new RobotId(1), missionEvent.RobotId);
        Assert.False(missionEvent.IsFault);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_event_code_throws(string eventCode)
    {
        Assert.Throws<ArgumentException>(() => MissionEvent.Create(
            new MissionEventId(1), 1, new RobotId(1), OccurredAtUtc, eventCode, "Mission dispatched"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<ArgumentException>(() => MissionEvent.Create(
            new MissionEventId(1), 1, new RobotId(1), OccurredAtUtc, "DISPATCHED", title));
    }

    [Fact]
    public void Create_with_null_robot_id_and_passport_only_recorded_by_user_id_succeeds()
    {
        var missionEvent = MissionEvent.Create(
            new MissionEventId(1), 1, robotId: null, OccurredAtUtc, "REQUESTED", "Mission requested",
            recordedByUserId: 42);

        Assert.Null(missionEvent.RobotId);
        Assert.Equal(42, missionEvent.RecordedByUserId);
    }
}
