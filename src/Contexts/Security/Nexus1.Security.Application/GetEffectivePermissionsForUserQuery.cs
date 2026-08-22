using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Security.Application;

public sealed record GetEffectivePermissionsForUserQuery(int ApplicationUserId) : IQuery<IReadOnlyList<EffectivePermissionDto>>;
