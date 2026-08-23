using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

/// <summary>Fleet-wide (all units), unlike GetActiveAlarmsForUnitQuery.</summary>
public sealed record GetActiveAlarmsQuery : IQuery<IReadOnlyList<ActiveAlarmSummaryDto>>;
