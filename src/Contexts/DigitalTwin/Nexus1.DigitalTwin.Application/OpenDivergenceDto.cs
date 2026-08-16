namespace Nexus1.DigitalTwin.Application;

/// <summary>Atlas C.6.8 query 3 projection: DetectedAtUtc, SignalTag, ModelVariable (nullable), ModeledValue, MeasuredValue, DeltaValue, Severity, Status.</summary>
public sealed record OpenDivergenceDto(
    DateTime DetectedAtUtc, string SignalTag, string? ModelVariable, double ModeledValue, double MeasuredValue,
    double DeltaValue, string Severity, string Status);
