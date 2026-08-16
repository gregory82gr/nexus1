using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Security.Application;

/// <summary>RolePermission's defining behavior (atlas C.2.4.11): "Grants or denies a permission through a role."</summary>
public sealed record GrantPermissionToRoleCommand(
    int ApplicationRoleId, int PermissionId, bool IsGranted = true, int? GrantedByUserId = null, DateTime? ExpiresAtUtc = null)
    : ICommand;
