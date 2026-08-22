namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Atlas verification query 3, verbatim: "read the policy in console form."</summary>
public sealed record PolicyGridEntryDto(int StateIndex, string StateCode, string BestActionCode, decimal BestQValue, decimal? ActionMargin);
