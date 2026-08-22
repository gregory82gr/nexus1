using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

public sealed class GetExercisesWithCorrectiveObservationsQueryHandlerTests : EmergencyPreparednessComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_exercise_with_its_corrective_observation_count()
    {
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await using var seedContext = CreateDbContext();
        await EmergencyPreparednessSeedHelper.SeedCoreAsync(corePlatformContext, radiationMonitoringContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetExercisesWithCorrectiveObservationsQueryHandler(new EfExercisesWithCorrectiveObservationsFinder(dbContext));

        var result = await handler.Handle(new GetExercisesWithCorrectiveObservationsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var exercise = Assert.Single(result.Value);
        Assert.Equal(EmergencyPreparednessSeedHelper.ExerciseCode, exercise.ExerciseCode);
        Assert.Equal(1, exercise.CorrectiveObservationCount);
    }
}
