using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>Measurement's defining behavior (ADR-019). Exactly one of NumericValue/TextValue must be supplied (CK_Instrumentation_Measurement_OneValue).</summary>
public sealed record RecordMeasurementCommand(
    int SignalId, int SignalQualityId, int MeasurementSourceId, DateTime TimestampUtc,
    double? NumericValue = null, string? TextValue = null, bool IsEstimated = false)
    : ICommand<long>;
