using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>Opens a SignalQualityEvent (ADR-019) — half of SignalQualityEvent's open/close lifecycle, paired with CloseSignalQualityEventCommand.</summary>
public sealed record OpenSignalQualityEventCommand(
    int SignalId, int SignalQualityId, DateTime StartedAtUtc, string? ReasonCode = null, string? Notes = null)
    : ICommand<long>;
