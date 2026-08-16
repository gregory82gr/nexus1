using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;

namespace Nexus1.CorePlatform.ComponentTests;

public sealed class GetCurrentDeploymentVersionsQueryHandlerTests : CorePlatformComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static GetCurrentDeploymentVersionsQueryHandler CreateHandler(CorePlatformDbContext dbContext) =>
        new(new EfDeploymentVersionFinder(dbContext));

    [Fact]
    public async Task Only_current_versions_are_returned()
    {
        await using (var seedContext = CreateDbContext())
        {
            var currentRuntime = DeploymentVersion.Create(
                new DeploymentVersionId(1), "Nexus1.ModularRuntime", DeploymentComponentType.ApiService, "2.1.0", NowUtc, isCurrent: true);
            var oldRuntime = DeploymentVersion.Create(
                new DeploymentVersionId(2), "Nexus1.ModularRuntime", DeploymentComponentType.ApiService, "2.0.0", NowUtc);
            var currentSchema = DeploymentVersion.Create(
                new DeploymentVersionId(3), "CorePlatform", DeploymentComponentType.Schema, "1.0.0", NowUtc, isCurrent: true);

            await seedContext.DeploymentVersions.AddRangeAsync(currentRuntime, oldRuntime, currentSchema);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetCurrentDeploymentVersionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.DoesNotContain(result.Value, v => v.VersionNumber == "2.0.0");
    }
}
