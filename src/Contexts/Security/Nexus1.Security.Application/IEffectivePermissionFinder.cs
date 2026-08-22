using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

/// <summary>Matches the atlas's own C.2.8 "authorization backbone" verification query: effective permissions for one user from active roles.</summary>
public interface IEffectivePermissionFinder
{
    Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(
        ApplicationUserId applicationUserId, DateTime nowUtc, CancellationToken cancellationToken);
}
