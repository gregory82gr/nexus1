using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;
using Nexus1.Security.Infrastructure.Persistence;

namespace Nexus1.Security.ComponentTests;

public sealed class AssignRoleToUserCommandHandlerTests : SecurityComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private AssignRoleToUserCommandHandler CreateHandler(SecurityDbContext dbContext) => new(
        new EfRepository<ApplicationUser, ApplicationUserId>(dbContext),
        new EfRepository<ApplicationRole, ApplicationRoleId>(dbContext),
        new EfUserRoleWriter(dbContext),
        UnitOfWork(dbContext),
        new FixedDateTimeProvider(NowUtc));

    private async Task SeedUserAndRoleAsync()
    {
        await using var seedContext = CreateDbContext();
        var status = UserStatus.Create(new UserStatusId(1), "ACTIVE", "Active", NowUtc);
        var roleType = RoleType.Create(new RoleTypeId(1), "OPERATOR", "Operator", NowUtc);
        await seedContext.UserStatuses.AddAsync(status);
        await seedContext.RoleTypes.AddAsync(roleType);
        await seedContext.ApplicationUsers.AddAsync(
            ApplicationUser.Create(new ApplicationUserId(1), status.Id, "operator1", "OPERATOR1", "Operator One", NowUtc));
        await seedContext.ApplicationRoles.AddAsync(
            ApplicationRole.Create(new ApplicationRoleId(1), roleType.Id, "Operator", "OPERATOR", NowUtc));
        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Assigning_a_role_to_an_existing_user_persists_it()
    {
        await SeedUserAndRoleAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new AssignRoleToUserCommand(1, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var userRole = await verifyContext.UserRoles.SingleAsync();
        Assert.Equal(new ApplicationUserId(1), userRole.ApplicationUserId);
        Assert.Equal(new ApplicationRoleId(1), userRole.ApplicationRoleId);
    }

    [Fact]
    public async Task Assigning_a_role_for_a_nonexistent_user_fails_without_writing_anything()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new AssignRoleToUserCommand(999, 1), CancellationToken.None);

        Assert.True(result.IsFailure);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.UserRoles.CountAsync());
    }

    [Fact]
    public async Task Assigning_the_same_role_twice_fails_the_second_time()
    {
        await SeedUserAndRoleAsync();

        await using (var dbContext = CreateDbContext())
        {
            var first = await CreateHandler(dbContext).Handle(new AssignRoleToUserCommand(1, 1), CancellationToken.None);
            Assert.True(first.IsSuccess);
        }

        await using var secondContext = CreateDbContext();
        var second = await CreateHandler(secondContext).Handle(new AssignRoleToUserCommand(1, 1), CancellationToken.None);

        Assert.True(second.IsFailure);
    }
}
