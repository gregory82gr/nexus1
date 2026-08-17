using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

/// <summary>Matches the atlas's own C.12.5.2 query 4, verbatim: MissionReadinessItem rows where IsBlocking = 1 and status is BLOCKED/EXPIRED.</summary>
public sealed class GetBlockingReadinessFailuresQueryHandlerTests : RoboticsComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_blocking_items_with_blocked_or_expired_status()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        long missionId;
        await using (var readinessSeedContext = CreateDbContext())
        {
            var mission = Mission.Create(
                new MissionId(1), seed.UnitId, new MissionTypeId(seed.MissionTypeId), new MissionStatusId(seed.MissionStatusId),
                new MissionPriorityId(seed.MissionPriorityId), "MSN-2026-0001", "Reactor building inspection", NowUtc);
            await readinessSeedContext.Missions.AddAsync(mission);
            await readinessSeedContext.SaveChangesAsync();
            missionId = mission.Id.Value;

            var assessment = MissionReadinessAssessment.Create(
                new MissionReadinessAssessmentId(1), missionId, new ReadinessStatusId(seed.ReadinessStatusBlockedId), NowUtc);
            await readinessSeedContext.MissionReadinessAssessments.AddAsync(assessment);
            await readinessSeedContext.SaveChangesAsync();

            var blockingBattery = MissionReadinessItem.Create(
                new MissionReadinessItemId(1), assessment.Id.Value, new ReadinessStatusId(seed.ReadinessStatusBlockedId),
                "Battery check", detail: "Battery below threshold", isBlocking: true);
            var blockingExpired = MissionReadinessItem.Create(
                new MissionReadinessItemId(2), assessment.Id.Value, new ReadinessStatusId(seed.ReadinessStatusExpiredId),
                "Calibration check", detail: "Calibration certificate expired", isBlocking: true);
            var nonBlockingReady = MissionReadinessItem.Create(
                new MissionReadinessItemId(3), assessment.Id.Value, new ReadinessStatusId(seed.ReadinessStatusOkId),
                "Comms check", isBlocking: true);
            var advisoryOnly = MissionReadinessItem.Create(
                new MissionReadinessItemId(4), assessment.Id.Value, new ReadinessStatusId(seed.ReadinessStatusBlockedId),
                "Weather advisory", isBlocking: false);

            await readinessSeedContext.MissionReadinessItems.AddRangeAsync(
                blockingBattery, blockingExpired, nonBlockingReady, advisoryOnly);
            await readinessSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetBlockingReadinessFailuresQueryHandler(new EfBlockingReadinessFailuresFinder(dbContext));

        var result = await handler.Handle(new GetBlockingReadinessFailuresQuery(missionId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, f => f.CheckName == "Battery check");
        Assert.Contains(result.Value, f => f.CheckName == "Calibration check");
        Assert.DoesNotContain(result.Value, f => f.CheckName == "Comms check");
        Assert.DoesNotContain(result.Value, f => f.CheckName == "Weather advisory");
    }
}
