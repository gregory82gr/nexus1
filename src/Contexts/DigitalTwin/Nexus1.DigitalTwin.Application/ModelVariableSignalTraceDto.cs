namespace Nexus1.DigitalTwin.Application;

/// <summary>Atlas C.6.8 query 2 projection: TwinCode, ModelVariable, SignalTag, BindingRole, BindingStatus.</summary>
public sealed record ModelVariableSignalTraceDto(string TwinCode, string ModelVariable, string SignalTag, string BindingRole, string BindingStatus);
