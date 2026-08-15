using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application;

public sealed record OpenAnalysisCommand(int UnitId, long AlarmFloodId, string OpenedBy) : ICommand<long>;
