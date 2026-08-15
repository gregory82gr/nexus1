using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application;

public sealed record RejectHypothesisCommand(long AnalysisId, int HypothesisId, string Reason) : ICommand;
