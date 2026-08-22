using Nexus1.BuildingBlocks.Application;
using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

public sealed class RegisterRobotCommandHandlerTests : RoboticsComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static RegisterRobotCommandHandler CreateHandler(RoboticsDbContext dbContext) => new(
        new EfRepository<Robot, RobotId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Registers_a_new_robot_against_the_seeded_model_and_status()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RegisterRobotCommand(seed.RobotModelId, seed.RobotStatusAvailableId, "RBT-002", "Scout Two"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Robots.FindAsync(new RobotId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("RBT-002", stored!.Code);
        Assert.Null(stored.HomeUnitId);
    }

    [Fact]
    public async Task Registers_a_robot_with_a_home_unit_and_no_enforced_fk_beyond_the_seeded_unit()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RegisterRobotCommand(
                seed.RobotModelId, seed.RobotStatusAvailableId, "RBT-003", "Scout Three", HomeUnitId: seed.UnitId,
                SerialNumber: "SN-99900"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Robots.FindAsync(new RobotId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(seed.UnitId, stored!.HomeUnitId);
        Assert.Equal("SN-99900", stored.SerialNumber);
    }
}
