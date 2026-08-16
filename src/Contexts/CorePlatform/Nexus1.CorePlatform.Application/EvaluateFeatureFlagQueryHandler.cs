using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

public sealed class EvaluateFeatureFlagQueryHandler(IFeatureFlagFinder featureFlagFinder, IDateTimeProvider dateTimeProvider)
    : IQueryHandler<EvaluateFeatureFlagQuery, bool>
{
    public async Task<Result<bool>> Handle(EvaluateFeatureFlagQuery query, CancellationToken cancellationToken)
    {
        var flag = await featureFlagFinder.FindByCodeAsync(query.Code, query.EnvironmentName, cancellationToken);

        // Fail-closed: an unknown flag code evaluates to "not enabled," never an
        // error — a caller gating a capability on a flag should never be blocked
        // by a typo or a not-yet-seeded flag row turning into an exception.
        return Result<bool>.Success(flag is not null && flag.IsActiveAt(dateTimeProvider.UtcNow));
    }
}
