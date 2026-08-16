using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Security.Application;

/// <summary>UserRole's defining behavior (atlas C.2.4.11).</summary>
public sealed record AssignRoleToUserCommand(int ApplicationUserId, int ApplicationRoleId, int? AssignedByUserId = null, DateTime? ExpiresAtUtc = null)
    : ICommand;
