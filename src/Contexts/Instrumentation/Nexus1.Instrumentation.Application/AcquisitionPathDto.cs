namespace Nexus1.Instrumentation.Application;

/// <summary>Atlas C.5.8 query 4 projection: Tag, node code, Protocol, Endpoint, RawAddress.</summary>
public sealed record AcquisitionPathDto(string Tag, string NodeCode, string Protocol, string? Endpoint, string RawAddress);
