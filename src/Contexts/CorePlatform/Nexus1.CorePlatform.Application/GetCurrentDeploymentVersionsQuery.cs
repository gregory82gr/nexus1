using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

/// <summary>Matches the atlas's own C.1.8 verification query verbatim: "Which platform components are currently deployed?"</summary>
public sealed record GetCurrentDeploymentVersionsQuery : IQuery<IReadOnlyList<DeploymentVersionDto>>;
