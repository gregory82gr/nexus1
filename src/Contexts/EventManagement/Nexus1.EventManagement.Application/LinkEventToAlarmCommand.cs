using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>EventAlarmLink's defining behavior (ADR-022): connects an OperationalEvent to an alarm event that triggered or supported it.</summary>
public sealed record LinkEventToAlarmCommand(long OperationalEventId, long AlarmEventId, string LinkRole = "SUPPORTING", string? Note = null)
    : ICommand<long>;
