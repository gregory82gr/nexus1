using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Atlas verification query 2, verbatim: QTable WHERE IsFinal = 1 JOIN QTableEntry, GROUP BY QTable.Code, COUNT.</summary>
public sealed record GetFinalQTableEntryCountQuery : IQuery<IReadOnlyList<FinalQTableEntryCountDto>>;
