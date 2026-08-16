using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;
using Nexus1.Security.Infrastructure.Persistence;

namespace Nexus1.Security.ComponentTests;

public sealed class GrantPermissionToRoleCommandHandlerTests : SecurityComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private GrantPermissionToRoleCommandHandler CreateHandler(SecurityDbContext dbContext) => new(
        new EfRepository<ApplicationRole, ApplicationRoleId>(dbContext),
        new EfRepository<Permission, PermissionId>(dbContext),
        new EfRolePermissionWriter(dbContext),
        UnitOfWork(dbContext),
        new FixedDateTimeProvider(NowUtc));

    private async Task SeedRoleAndPermissionAsync()
    {
        await using var seedContext = CreateDbContext();
        var roleType = RoleType.Create(new RoleTypeId(1), "OPERATOR", "Operator", NowUtc);
        var category = PermissionCategory.Create(new PermissionCategoryId(1), "ALARM", "Alarm", NowUtc);
        await seedContext.RoleTypes.AddAsync(roleType);
        await seedContext.PermissionCategories.AddAsync(category);
        await seedContext.ApplicationRoles.AddAsync(
            ApplicationRole.Create(new ApplicationRoleId(1), roleType.Id, "Operator", "OPERATOR", NowUtc));
        await seedContext.Permissions.AddAsync(
            Permission.Create(new PermissionId(1), category.Id, "alarm.acknowledge", "Acknowledge Alarm", "Acknowledge", NowUtc));
        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Granting_a_permission_to_an_existing_role_persists_it()
    {
        await SeedRoleAndPermissionAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GrantPermissionToRoleCommand(1, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var rolePermission = await verifyContext.RolePermissions.SingleAsync();
        Assert.True(rolePermission.IsGranted);
    }

    [Fact]
    public async Task Granting_a_permission_for_a_nonexistent_role_fails()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GrantPermissionToRoleCommand(999, 1), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
