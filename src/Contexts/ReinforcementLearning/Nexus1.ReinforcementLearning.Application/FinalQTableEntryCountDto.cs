namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Atlas verification query 2, verbatim: "a final Q-table should contain 175 state-action values."</summary>
public sealed record FinalQTableEntryCountDto(string QTableCode, int QValueCount);
