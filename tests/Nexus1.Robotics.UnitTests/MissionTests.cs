using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class MissionTests
{
    private static readonly DateTime RequestedAtUtc = new(2026, 8, 17, 7, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_null()
    {
        var mission = Mission.Create(
            new MissionId(1), unitId: 1, new MissionTypeId(1), new MissionStatusId(1), new MissionPriorityId(1),
            "MSN-2026-0001", "Reactor building inspection", RequestedAtUtc);

        Assert.Equal("MSN-2026-0001", mission.Code);
        Assert.Equal("Reactor building inspection", mission.Title);
        Assert.Equal(RequestedAtUtc, mission.RequestedAtUtc);
        Assert.Null(mission.PlannedStartUtc);
        Assert.Null(mission.RequestedByUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Mission.Create(
            new MissionId(1), 1, new MissionTypeId(1), new MissionStatusId(1), new MissionPriorityId(1),
            code, "Reactor building inspection", RequestedAtUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<ArgumentException>(() => Mission.Create(
            new MissionId(1), 1, new MissionTypeId(1), new MissionStatusId(1), new MissionPriorityId(1),
            "MSN-2026-0001", title, RequestedAtUtc));
    }

    [Fact]
    public void Create_with_passport_only_ids_sets_them_with_no_enforced_fk()
    {
        var mission = Mission.Create(
            new MissionId(1), 1, new MissionTypeId(1), new MissionStatusId(1), new MissionPriorityId(1),
            "MSN-2026-0001", "Reactor building inspection", RequestedAtUtc,
            requestedByUserId: 10, approvedByUserId: 11);

        Assert.Equal(10, mission.RequestedByUserId);
        Assert.Equal(11, mission.ApprovedByUserId);
    }
}
