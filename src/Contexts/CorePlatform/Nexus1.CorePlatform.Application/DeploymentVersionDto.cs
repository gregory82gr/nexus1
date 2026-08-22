namespace Nexus1.CorePlatform.Application;

public sealed record DeploymentVersionDto(
    int DeploymentVersionId, string ComponentName, string ComponentType, string VersionNumber, DateTime? ReleaseDateUtc);
