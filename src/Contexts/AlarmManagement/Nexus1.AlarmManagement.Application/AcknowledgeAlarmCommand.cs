using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed record AcknowledgeAlarmCommand(long AlarmEventId, Guid AcknowledgedByUserId) : ICommand;
