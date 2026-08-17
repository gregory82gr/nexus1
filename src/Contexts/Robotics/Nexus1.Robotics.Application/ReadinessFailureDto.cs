namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 4, verbatim: readiness failures that block dispatch.</summary>
public sealed record ReadinessFailureDto(string CheckName, string ReadinessStatus, string? Detail);
