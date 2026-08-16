using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

public sealed class GetCurrentDeploymentVersionsQueryHandler(IDeploymentVersionFinder deploymentVersionFinder)
    : IQueryHandler<GetCurrentDeploymentVersionsQuery, IReadOnlyList<DeploymentVersionDto>>
{
    public async Task<Result<IReadOnlyList<DeploymentVersionDto>>> Handle(
        GetCurrentDeploymentVersionsQuery query, CancellationToken cancellationToken)
    {
        var versions = await deploymentVersionFinder.GetCurrentAsync(cancellationToken);

        IReadOnlyList<DeploymentVersionDto> dtos = versions
            .Select(v => new DeploymentVersionDto(v.Id.Value, v.ComponentName, v.ComponentType.ToString(), v.VersionNumber, v.ReleaseDateUtc))
            .ToList();

        return Result<IReadOnlyList<DeploymentVersionDto>>.Success(dtos);
    }
}
