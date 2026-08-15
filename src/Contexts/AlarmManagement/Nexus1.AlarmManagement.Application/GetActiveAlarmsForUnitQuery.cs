using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed record GetActiveAlarmsForUnitQuery(int UnitId) : IQuery<IReadOnlyList<ActiveAlarmDto>>;
