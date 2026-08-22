using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;

namespace Nexus1.CorePlatform.ComponentTests;

public sealed class UpdateAppSettingValueCommandHandlerTests : CorePlatformComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static UpdateAppSettingValueCommandHandler CreateHandler(CorePlatformDbContext dbContext) =>
        new(new EfAppSettingFinder(dbContext), UnitOfWork(dbContext));

    private async Task SeedAppSettingAsync()
    {
        await using var seedContext = CreateDbContext();
        var setting = AppSetting.Create(new AppSettingId(1), "flood.threshold", "3", AppSettingValueType.Int, NowUtc);
        await seedContext.AppSettings.AddAsync(setting);
        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Updating_an_existing_setting_persists_the_new_value()
    {
        await SeedAppSettingAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new UpdateAppSettingValueCommand("flood.threshold", "5"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var setting = await verifyContext.AppSettings.SingleAsync(s => s.Key == "flood.threshold");
        Assert.Equal("5", setting.Value);
    }

    [Fact]
    public async Task Updating_a_nonexistent_setting_fails_without_writing_anything()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new UpdateAppSettingValueCommand("does.not.exist", "5"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("does.not.exist", result.Error);
    }
}
