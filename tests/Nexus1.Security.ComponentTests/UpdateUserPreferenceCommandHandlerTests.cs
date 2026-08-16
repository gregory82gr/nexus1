using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;
using Nexus1.Security.Infrastructure.Persistence;

namespace Nexus1.Security.ComponentTests;

public sealed class UpdateUserPreferenceCommandHandlerTests : SecurityComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private UpdateUserPreferenceCommandHandler CreateHandler(SecurityDbContext dbContext) => new(
        new EfRepository<ApplicationUser, ApplicationUserId>(dbContext),
        new EfRepository<UserPreference, ApplicationUserId>(dbContext),
        UnitOfWork(dbContext),
        new FixedDateTimeProvider(NowUtc));

    private async Task SeedUserAsync()
    {
        await using var seedContext = CreateDbContext();
        var status = UserStatus.Create(new UserStatusId(1), "ACTIVE", "Active", NowUtc);
        await seedContext.UserStatuses.AddAsync(status);
        await seedContext.ApplicationUsers.AddAsync(
            ApplicationUser.Create(new ApplicationUserId(1), status.Id, "operator1", "OPERATOR1", "Operator One", NowUtc));
        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task First_call_creates_the_preference_row()
    {
        await SeedUserAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new UpdateUserPreferenceCommand(1, LanguageId: 2, TimeZoneId: 3, Theme: "Dark"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var preference = await verifyContext.UserPreferences.SingleAsync();
        Assert.Equal(2, preference.LanguageId);
        Assert.Equal("Dark", preference.Theme);
    }

    [Fact]
    public async Task Second_call_updates_the_existing_row_rather_than_duplicating()
    {
        await SeedUserAsync();

        await using (var dbContext = CreateDbContext())
        {
            await CreateHandler(dbContext).Handle(new UpdateUserPreferenceCommand(1, 1, 1, "Light"), CancellationToken.None);
        }

        await using (var dbContext = CreateDbContext())
        {
            var result = await CreateHandler(dbContext).Handle(new UpdateUserPreferenceCommand(1, 2, 2, "Dark"), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.UserPreferences.CountAsync());
        var preference = await verifyContext.UserPreferences.SingleAsync();
        Assert.Equal("Dark", preference.Theme);
    }

    [Fact]
    public async Task Update_for_a_nonexistent_user_fails()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new UpdateUserPreferenceCommand(999, 1, 1), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
