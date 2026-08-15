using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application;

public sealed record CloseAnalysisCommand(long AnalysisId, string Verdict, string ClosedBy) : ICommand;
