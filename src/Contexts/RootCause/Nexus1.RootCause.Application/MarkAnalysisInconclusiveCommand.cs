using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application;

/// <summary>
/// Terminally marks a root-cause case Inconclusive (ADR-039) — "investigated, cannot
/// conclude." Requires only a reason; no verdict. Parallel to CloseAnalysisCommand.
/// </summary>
public sealed record MarkAnalysisInconclusiveCommand(long AnalysisId, string Reason, string DecidedBy) : ICommand;
