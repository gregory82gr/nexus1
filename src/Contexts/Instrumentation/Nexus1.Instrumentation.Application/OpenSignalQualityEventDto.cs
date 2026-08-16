namespace Nexus1.Instrumentation.Application;

/// <summary>Atlas C.5.8 query 3 projection: Tag, quality code, StartedAtUtc, EndedAtUtc, ReasonCode.</summary>
public sealed record OpenSignalQualityEventDto(string Tag, string QualityCode, DateTime StartedAtUtc, DateTime? EndedAtUtc, string? ReasonCode);
