namespace Nexus1.Instrumentation.Application;

/// <summary>Atlas C.5.8 query 2 projection: TimestampUtc, NumericValue, quality code, source code.</summary>
public sealed record LatestMeasurementDto(DateTime TimestampUtc, double? NumericValue, string QualityCode, string SourceCode);
