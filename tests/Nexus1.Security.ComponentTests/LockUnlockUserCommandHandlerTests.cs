using Microsoft.EntityFrameworkCore;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;
using Nexus1.Security.Infrastructure.Persistence;

namespace Nexus1.Security.ComponentTests;

public sealed class LockUnlockUserCommandHandlerTests : SecurityComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

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
    public async Task LockUserCommand_persists_the_lockout_end()
    {
        await SeedUserAsync();

        await using var dbContext = CreateDbContext();
        var handler = new LockUserCommandHandler(new EfRepository<ApplicationUser, ApplicationUserId>(dbContext), UnitOfWork(dbContext));
        var result = await handler.Handle(new LockUserCommand(1, NowUtc.AddDays(1)), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var user = await verifyContext.ApplicationUsers.SingleAsync();
        Assert.True(user.IsLockedOut(NowUtc.AddHours(1)));
    }

    [Fact]
    public async Task UnlockUserCommand_clears_a_prior_lockout()
    {
        await SeedUserAsync();

        await using (var dbContext = CreateDbContext())
        {
            var lockHandler = new LockUserCommandHandler(new EfRepository<ApplicationUser, ApplicationUserId>(dbContext), UnitOfWork(dbContext));
            await lockHandler.Handle(new LockUserCommand(1, NowUtc.AddDays(1)), CancellationToken.None);
        }

        await using (var dbContext = CreateDbContext())
        {
            var unlockHandler = new UnlockUserCommandHandler(new EfRepository<ApplicationUser, ApplicationUserId>(dbContext), UnitOfWork(dbContext));
            var result = await unlockHandler.Handle(new UnlockUserCommand(1), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using var verifyContext = CreateDbContext();
        var user = await verifyContext.ApplicationUsers.SingleAsync();
        Assert.False(user.IsLockedOut(NowUtc.AddHours(1)));
    }

    [Fact]
    public async Task LockUserCommand_for_a_nonexistent_user_fails()
    {
        await using var dbContext = CreateDbContext();
        var handler = new LockUserCommandHandler(new EfRepository<ApplicationUser, ApplicationUserId>(dbContext), UnitOfWork(dbContext));
        var result = await handler.Handle(new LockUserCommand(999, NowUtc.AddDays(1)), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
