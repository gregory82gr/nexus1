namespace Nexus1.DigitalTwin.Application;

/// <summary>Atlas C.6.8 query 1 projection: UnitCode, TwinCode, ModelType, Status, Fidelity.</summary>
public sealed record ActiveTwinDto(string UnitCode, string TwinCode, string ModelType, string Status, string Fidelity);
