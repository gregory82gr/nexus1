using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

/// <summary>
/// Atlas verification query 4, verbatim: AdvisoryRecommendation WHERE
/// WasClamped = 1, JOIN StateDefinition, JOIN ActionDefinition for the
/// recommended action, LEFT JOIN ActionDefinition again for the clamped
/// action, ORDER BY RequestedAtUtc DESC.
/// </summary>
public sealed record GetClampedRecommendationsQuery : IQuery<IReadOnlyList<ClampedRecommendationDto>>;
