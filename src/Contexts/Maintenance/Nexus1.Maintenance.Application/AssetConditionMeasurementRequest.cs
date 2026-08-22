namespace Nexus1.Maintenance.Application;

/// <summary>One measurement row inside a RecordAssetConditionCommand. SignalId is nullable (Instrumentation.Signal, real FK); EngineeringUnitId is required (CorePlatform.EngineeringUnit, real FK).</summary>
public sealed record AssetConditionMeasurementRequest(
    int EngineeringUnitId, double MeasuredValue, DateTime MeasuredAtUtc, int? SignalId = null, string? MeasurementNote = null);
