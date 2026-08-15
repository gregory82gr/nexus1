using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application;

public sealed record AddHypothesisCommand(long AnalysisId, string HypothesisStatement) : ICommand<int>;
