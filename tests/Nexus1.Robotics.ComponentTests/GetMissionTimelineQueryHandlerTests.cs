using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

/// <summary>Matches the atlas's own C.12.5.2 query 3, verbatim: mission timeline for one mission, ordered by OccurredAtUtc, with the acting robot's code.</summary>
public sealed class GetMissionTimelineQueryHandlerTests : RoboticsComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_mission_events_ordered_by_occurrence_time_with_acting_robot_code()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        long missionId;
        await using (var missionSeedContext = CreateDbContext())
        {
            var mission = Mission.Create(
                new MissionId(1), seed.UnitId, new MissionTypeId(seed.MissionTypeId), new MissionStatusId(seed.MissionStatusId),
                new MissionPriorityId(seed.MissionPriorityId), "MSN-2026-0001", "Reactor building inspection", NowUtc);
            await missionSeedContext.Missions.AddAsync(mission);
            await missionSeedContext.SaveChangesAsync();
            missionId = mission.Id.Value;

            var laterEvent = MissionEvent.Create(
                new MissionEventId(1), missionId, new RobotId(seed.RobotId), NowUtc.AddMinutes(10), "ARRIVED", "Robot arrived on site");
            var earlierEvent = MissionEvent.Create(
                new MissionEventId(2), missionId, new RobotId(seed.RobotId), NowUtc, "DISPATCHED", "Mission dispatched");

            await missionSeedContext.MissionEvents.AddAsync(laterEvent);
            await missionSeedContext.MissionEvents.AddAsync(earlierEvent);
            await missionSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetMissionTimelineQueryHandler(new EfMissionTimelineFinder(dbContext));

        var result = await handler.Handle(new GetMissionTimelineQuery(missionId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("DISPATCHED", result.Value[0].EventCode);
        Assert.Equal("ARRIVED", result.Value[1].EventCode);
        Assert.All(result.Value, e => Assert.Equal(RoboticsSeedHelper.RobotCode, e.RobotCode));
    }
}
