using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

/// <summary>Matches the atlas's own C.12.5.2 query 1, verbatim: robots currently available by unit.</summary>
public sealed class GetAvailableRobotsByUnitQueryHandlerTests : RoboticsComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_available_robots_with_their_unit_code_and_excludes_non_available_robots()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        await using (var robotSeedContext = CreateDbContext())
        {
            var unavailableRobot = Robot.Create(
                new RobotId(2), new RobotModelId(seed.RobotModelId), new RobotStatusId(seed.RobotStatusOtherId),
                "RBT-002", "Scout Two", homeUnitId: seed.UnitId);
            await robotSeedContext.Robots.AddAsync(unavailableRobot);
            await robotSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetAvailableRobotsByUnitQueryHandler(new EfAvailableRobotsFinder(dbContext));

        var result = await handler.Handle(new GetAvailableRobotsByUnitQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var available = Assert.Single(result.Value);
        Assert.Equal(RoboticsSeedHelper.RobotCode, available.RobotCode);
        Assert.Equal(RoboticsSeedHelper.UnitCode, available.UnitCode);
        Assert.Equal("AVAILABLE", available.Status);
        Assert.DoesNotContain(result.Value, r => r.RobotCode == "RBT-002");
    }
}
