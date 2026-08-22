using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Atlas verification query 1, verbatim: Policy JOIN PolicyEntry, GROUP BY Policy.Code, COUNT.</summary>
public sealed record GetPolicyEntryCountQuery : IQuery<IReadOnlyList<PolicyEntryCountDto>>;
