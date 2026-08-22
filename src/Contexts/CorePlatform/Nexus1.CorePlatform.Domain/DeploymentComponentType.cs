namespace Nexus1.CorePlatform.Domain;

/// <summary>Matches the atlas's CK_CorePlatform_Version_ComponentType check constraint exactly (C.1.4.9).</summary>
public enum DeploymentComponentType
{
    Console,
    Schema,
    SeedData,
    ApiService,
    Worker,
    Documentation,
}
