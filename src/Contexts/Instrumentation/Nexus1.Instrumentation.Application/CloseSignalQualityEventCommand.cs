using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>Closes an open SignalQualityEvent (ADR-019) — re-validates CK_Instrumentation_SignalQualityEvent_Time via SignalQualityEvent.Close.</summary>
public sealed record CloseSignalQualityEventCommand(long SignalQualityEventId, DateTime EndedAtUtc, string? ReasonCode = null) : ICommand;
