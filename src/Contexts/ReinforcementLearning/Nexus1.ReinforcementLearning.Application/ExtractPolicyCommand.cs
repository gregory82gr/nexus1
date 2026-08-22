using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Application;

/// <summary>Policy's defining behavior (ADR-026): extracts a new readable policy from a final QTable against its status.</summary>
public sealed record ExtractPolicyCommand(
    int QTableId, int PolicyStatusId, string Code, string Name, DateTime ExtractedAtUtc, int EntryCount)
    : ICommand<int>;
