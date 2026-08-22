namespace Nexus1.Maintenance.Application;

/// <summary>One trend point row inside a RecordDegradationCommand. EngineeringUnitId is required (CorePlatform.EngineeringUnit, real FK); SourceSignalId is nullable (Instrumentation.Signal, real FK).</summary>
public sealed record DegradationTrendPointRequest(
    int EngineeringUnitId, DateTime MeasuredAtUtc, double Value, int? SourceSignalId = null, string? Note = null);
