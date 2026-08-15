using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

/// <summary>
/// CountThreshold/WindowSeconds are required caller inputs, not defaults —
/// neither source states a number (ADR-004); inventing one here would be
/// exactly the "vibe architecture" this project prohibits.
/// </summary>
public sealed record DetectFloodCommand(int UnitId, int CountThreshold, int WindowSeconds) : ICommand<long?>;
