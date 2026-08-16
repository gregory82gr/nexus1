using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

public sealed class GetEffectivePermissionsForUserQueryHandler(IEffectivePermissionFinder finder, IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetEffectivePermissionsForUserQuery, IReadOnlyList<EffectivePermissionDto>>
{
    public async Task<Result<IReadOnlyList<EffectivePermissionDto>>> Handle(
        GetEffectivePermissionsForUserQuery query, CancellationToken cancellationToken)
    {
        var permissions = await finder.GetEffectivePermissionsAsync(
            new ApplicationUserId(query.ApplicationUserId), dateTimeProvider.UtcNow, cancellationToken);

        return Result<IReadOnlyList<EffectivePermissionDto>>.Success(permissions);
    }
}
