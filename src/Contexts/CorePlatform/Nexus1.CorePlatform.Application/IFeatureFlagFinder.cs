using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Application;

/// <summary>Looks up by (Code, EnvironmentName) — the atlas's own natural key (UQ_CorePlatform_FeatureFlag_Code_Environment).</summary>
public interface IFeatureFlagFinder
{
    Task<FeatureFlag?> FindByCodeAsync(string code, string environmentName, CancellationToken cancellationToken);
}
