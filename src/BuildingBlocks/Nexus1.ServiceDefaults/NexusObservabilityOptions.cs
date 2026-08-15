namespace Nexus1.ServiceDefaults;

/// <summary>
/// The subset of ch.51's resource-identity fields (Executable Asset 51-D)
/// this project actually has a source for today — no CI build digest or
/// deployment manifest exists yet, so ServiceInstanceId/BuildIdentity are
/// left for a later step rather than faked.
/// </summary>
public sealed record NexusObservabilityOptions(string ServiceName, Uri OtlpEndpoint);
