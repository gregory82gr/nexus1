using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;
using Nexus1.Security.Infrastructure.Persistence;

namespace Nexus1.Security.ComponentTests;

/// <summary>Matches the atlas's own C.2.8 "authorization backbone" verification query.</summary>
public sealed class GetEffectivePermissionsForUserQueryHandlerTests : SecurityComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private GetEffectivePermissionsForUserQueryHandler CreateHandler(SecurityDbContext dbContext) =>
        new(new EfEffectivePermissionFinder(dbContext), new FixedDateTimeProvider(NowUtc));

    [Fact]
    public async Task Returns_permissions_from_an_active_unexpired_role_assignment()
    {
        await using (var seedContext = CreateDbContext())
        {
            var status = UserStatus.Create(new UserStatusId(1), "ACTIVE", "Active", NowUtc);
            var roleType = RoleType.Create(new RoleTypeId(1), "OPERATOR", "Operator", NowUtc);
            var category = PermissionCategory.Create(new PermissionCategoryId(1), "ALARM", "Alarm", NowUtc);
            await seedContext.UserStatuses.AddAsync(status);
            await seedContext.RoleTypes.AddAsync(roleType);
            await seedContext.PermissionCategories.AddAsync(category);

            var user = ApplicationUser.Create(new ApplicationUserId(1), status.Id, "operator1", "OPERATOR1", "Operator One", NowUtc);
            var role = ApplicationRole.Create(new ApplicationRoleId(1), roleType.Id, "Operator", "OPERATOR", NowUtc);
            var permission = Permission.Create(new PermissionId(1), category.Id, "alarm.acknowledge", "Acknowledge Alarm", "Acknowledge", NowUtc);
            await seedContext.ApplicationUsers.AddAsync(user);
            await seedContext.ApplicationRoles.AddAsync(role);
            await seedContext.Permissions.AddAsync(permission);

            await seedContext.UserRoles.AddAsync(new UserRole(user.Id, role.Id, NowUtc));
            await seedContext.RolePermissions.AddAsync(new RolePermission(role.Id, permission.Id, NowUtc));
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetEffectivePermissionsForUserQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var effectivePermission = Assert.Single(result.Value!);
        Assert.Equal("alarm.acknowledge", effectivePermission.PermissionCode);
        Assert.Equal("ALARM", effectivePermission.CategoryCode);
        Assert.True(effectivePermission.IsGranted);
    }

    [Fact]
    public async Task Excludes_permissions_from_an_expired_role_assignment()
    {
        await using (var seedContext = CreateDbContext())
        {
            var status = UserStatus.Create(new UserStatusId(1), "ACTIVE", "Active", NowUtc);
            var roleType = RoleType.Create(new RoleTypeId(1), "OPERATOR", "Operator", NowUtc);
            var category = PermissionCategory.Create(new PermissionCategoryId(1), "ALARM", "Alarm", NowUtc);
            await seedContext.UserStatuses.AddAsync(status);
            await seedContext.RoleTypes.AddAsync(roleType);
            await seedContext.PermissionCategories.AddAsync(category);

            var user = ApplicationUser.Create(new ApplicationUserId(1), status.Id, "operator1", "OPERATOR1", "Operator One", NowUtc.AddDays(-10));
            var role = ApplicationRole.Create(new ApplicationRoleId(1), roleType.Id, "Operator", "OPERATOR", NowUtc);
            var permission = Permission.Create(new PermissionId(1), category.Id, "alarm.acknowledge", "Acknowledge Alarm", "Acknowledge", NowUtc);
            await seedContext.ApplicationUsers.AddAsync(user);
            await seedContext.ApplicationRoles.AddAsync(role);
            await seedContext.Permissions.AddAsync(permission);

            // Assigned in the past, already expired as of NowUtc.
            await seedContext.UserRoles.AddAsync(new UserRole(
                user.Id, role.Id, NowUtc.AddDays(-5), expiresAtUtc: NowUtc.AddDays(-1)));
            await seedContext.RolePermissions.AddAsync(new RolePermission(role.Id, permission.Id, NowUtc));
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetEffectivePermissionsForUserQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Reports_a_revoked_role_permission_as_present_but_not_granted()
    {
        await using (var seedContext = CreateDbContext())
        {
            var status = UserStatus.Create(new UserStatusId(1), "ACTIVE", "Active", NowUtc);
            var roleType = RoleType.Create(new RoleTypeId(1), "OPERATOR", "Operator", NowUtc);
            var category = PermissionCategory.Create(new PermissionCategoryId(1), "ALARM", "Alarm", NowUtc);
            await seedContext.UserStatuses.AddAsync(status);
            await seedContext.RoleTypes.AddAsync(roleType);
            await seedContext.PermissionCategories.AddAsync(category);

            var user = ApplicationUser.Create(new ApplicationUserId(1), status.Id, "operator1", "OPERATOR1", "Operator One", NowUtc);
            var role = ApplicationRole.Create(new ApplicationRoleId(1), roleType.Id, "Operator", "OPERATOR", NowUtc);
            var permission = Permission.Create(new PermissionId(1), category.Id, "alarm.acknowledge", "Acknowledge Alarm", "Acknowledge", NowUtc);
            await seedContext.ApplicationUsers.AddAsync(user);
            await seedContext.ApplicationRoles.AddAsync(role);
            await seedContext.Permissions.AddAsync(permission);

            await seedContext.UserRoles.AddAsync(new UserRole(user.Id, role.Id, NowUtc));
            var grant = new RolePermission(role.Id, permission.Id, NowUtc);
            grant.Revoke();
            await seedContext.RolePermissions.AddAsync(grant);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetEffectivePermissionsForUserQuery(1), CancellationToken.None);

        // Matches the atlas's own C.2.8 query shape: it selects rp.IsGranted
        // rather than filtering WHERE IsGranted = 1, so an explicit deny is
        // visible to the caller, not silently dropped from the result.
        Assert.True(result.IsSuccess);
        var effectivePermission = Assert.Single(result.Value!);
        Assert.False(effectivePermission.IsGranted);
    }
}
