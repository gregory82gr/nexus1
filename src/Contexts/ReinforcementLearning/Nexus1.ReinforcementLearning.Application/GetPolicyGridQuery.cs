using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Atlas verification query 3, verbatim: PolicyEntry JOIN StateDefinition/ActionDefinition, ORDER BY StateIndex.</summary>
public sealed record GetPolicyGridQuery(int PolicyId) : IQuery<IReadOnlyList<PolicyGridEntryDto>>;
