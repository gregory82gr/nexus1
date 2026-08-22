using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

/// <summary>TwinDivergence's defining behavior (ADR-020) — the handler lets the domain compute DeltaValue (MeasuredValue - ModeledValue), never accepting it as an independent parameter here either.</summary>
public sealed record RecordTwinDivergenceCommand(
    long TwinSnapshotId, int SignalId, int DivergenceSeverityId, int DivergenceStatusId, DateTime DetectedAtUtc,
    double ModeledValue, double MeasuredValue, int? TwinVariableId = null, decimal? DeltaPercent = null,
    double? ThresholdAbs = null, decimal? ThresholdPercent = null, string? Explanation = null)
    : ICommand<long>;
