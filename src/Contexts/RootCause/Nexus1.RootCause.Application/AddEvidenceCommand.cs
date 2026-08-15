using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application;

public sealed record AddEvidenceCommand(long AnalysisId, int HypothesisId, string Description) : ICommand;
