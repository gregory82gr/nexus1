using Nexus1.BuildingBlocks.Application;
using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Domain;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

public sealed class ScheduleExerciseCommandHandlerTests : EmergencyPreparednessComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static ScheduleExerciseCommandHandler CreateHandler(EmergencyPreparednessDbContext dbContext) => new(
        new EfRepository<Exercise, ExerciseId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Schedules_a_new_exercise_against_the_seeded_type_and_status_chain()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ScheduleExerciseCommand(
                "EX-002", "Radiological Release Tabletop", seed.ExerciseTypeId, seed.ExerciseStatusId,
                EmergencyPreparednessSeedHelper.SiteId, NowUtc.AddDays(14), NowUtc.AddDays(14).AddHours(2),
                CoordinatorUserId: 602),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Exercises.FindAsync(new ExerciseId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("EX-002", stored!.Code);
        Assert.Null(stored.ActualStartUtc);
        Assert.Null(stored.ActualEndUtc);
    }
}
