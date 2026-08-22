namespace Nexus1.Security.Application;

public sealed record EffectivePermissionDto(string PermissionCode, string PermissionName, string CategoryCode, bool IsGranted);
