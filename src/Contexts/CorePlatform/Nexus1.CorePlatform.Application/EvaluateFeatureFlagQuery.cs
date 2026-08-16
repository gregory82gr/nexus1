using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

/// <summary>The atlas's own defining behavior for FeatureFlag (C.1.4.4): "Switches capabilities on or off per environment, optionally with expiry."</summary>
public sealed record EvaluateFeatureFlagQuery(string Code, string EnvironmentName = "All") : IQuery<bool>;
