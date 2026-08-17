using Nexus1.BuildingBlocks.Application;
using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

public sealed class DispatchMissionCommandHandlerTests : RoboticsComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static DispatchMissionCommandHandler CreateHandler(RoboticsDbContext dbContext) => new(
        new EfRepository<Mission, MissionId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Dispatches_a_new_mission_against_the_seeded_unit_type_status_and_priority()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new DispatchMissionCommand(
                seed.UnitId, seed.MissionTypeId, seed.MissionStatusId, seed.MissionPriorityId, "MSN-2026-0001",
                "Reactor building inspection", NowUtc),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Missions.FindAsync(new MissionId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("MSN-2026-0001", stored!.Code);
        Assert.Equal(seed.UnitId, stored.UnitId);
    }

    [Fact]
    public async Task Dispatches_a_mission_with_passport_only_requester_and_approver_ids()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RoboticsSeedHelper.SeedCoreAsync(reactorFleetContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new DispatchMissionCommand(
                seed.UnitId, seed.MissionTypeId, seed.MissionStatusId, seed.MissionPriorityId, "MSN-2026-0002",
                "Containment sweep", NowUtc, RequestedByUserId: 10, ApprovedByUserId: 11),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Missions.FindAsync(new MissionId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(10, stored!.RequestedByUserId);
        Assert.Equal(11, stored.ApprovedByUserId);
    }
}
