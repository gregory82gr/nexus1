using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

/// <summary>EventFloodLink's defining behavior (ADR-022): connects an OperationalEvent to an alarm flood window.</summary>
public sealed record LinkEventToFloodCommand(long OperationalEventId, long AlarmFloodId, string LinkRole = "TRIGGER", string? Note = null)
    : ICommand<long>;
