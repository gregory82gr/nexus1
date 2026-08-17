using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

/// <summary>Matches the atlas's own C.12.5.2 query 2, verbatim: one row per robot, most recent RobotHealthSnapshot, with battery/communication status codes.</summary>
public sealed class GetLatestHealthSnapshotPerRobotQueryHandlerTests : RoboticsComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_the_most_recent_snapshot_per_robot()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        await using (var snapshotSeedContext = CreateDbContext())
        {
            var earlierSnapshot = RobotHealthSnapshot.Create(
                new RobotHealthSnapshotId(1), new RobotId(seed.RobotId), new BatteryStatusId(seed.BatteryStatusId),
                new CommunicationStatusId(seed.CommunicationStatusId), NowUtc.AddHours(-1), batteryPercent: 90m);
            var laterSnapshot = RobotHealthSnapshot.Create(
                new RobotHealthSnapshotId(2), new RobotId(seed.RobotId), new BatteryStatusId(seed.BatteryStatusId),
                new CommunicationStatusId(seed.CommunicationStatusId), NowUtc, batteryPercent: 78m);

            await snapshotSeedContext.RobotHealthSnapshots.AddAsync(earlierSnapshot);
            await snapshotSeedContext.RobotHealthSnapshots.AddAsync(laterSnapshot);
            await snapshotSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetLatestHealthSnapshotPerRobotQueryHandler(new EfLatestHealthSnapshotFinder(dbContext));

        var result = await handler.Handle(new GetLatestHealthSnapshotPerRobotQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var snapshot = Assert.Single(result.Value);
        Assert.Equal(RoboticsSeedHelper.RobotCode, snapshot.RobotCode);
        Assert.Equal(78m, snapshot.BatteryPercent);
        Assert.Equal(NowUtc, snapshot.SnapshotAtUtc);
    }
}
