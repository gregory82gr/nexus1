using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed record DefineAlarmCommand(int UnitId, string Code, string Name, AlarmSeverity Severity, decimal ThresholdValue)
    : ICommand<int>;
