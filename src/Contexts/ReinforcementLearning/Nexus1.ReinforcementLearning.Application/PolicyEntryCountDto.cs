namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Atlas verification query 1, verbatim: "the 35x5 policy should have one entry per state."</summary>
public sealed record PolicyEntryCountDto(string PolicyCode, int PolicyEntryCount);
