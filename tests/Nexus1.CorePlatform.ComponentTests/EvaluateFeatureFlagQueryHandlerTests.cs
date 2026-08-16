using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;

namespace Nexus1.CorePlatform.ComponentTests;

public sealed class EvaluateFeatureFlagQueryHandlerTests : CorePlatformComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private static EvaluateFeatureFlagQueryHandler CreateHandler(CorePlatformDbContext dbContext) =>
        new(new EfFeatureFlagFinder(dbContext), new FixedDateTimeProvider(NowUtc));

    [Fact]
    public async Task An_enabled_flag_within_its_active_window_evaluates_true()
    {
        await using (var seedContext = CreateDbContext())
        {
            var flag = FeatureFlag.Create(
                new FeatureFlagId(1), "new-atlas-ui", "New Atlas UI", NowUtc.AddDays(-1),
                isEnabled: true, enabledFromUtc: NowUtc.AddHours(-1));
            await seedContext.FeatureFlags.AddAsync(flag);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new EvaluateFeatureFlagQuery("new-atlas-ui"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task An_unknown_flag_code_evaluates_false_not_a_failure()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new EvaluateFeatureFlagQuery("does-not-exist"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }
}
