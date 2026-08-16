using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

/// <summary>Atlas C.6.8 query 2, verbatim: for a twin code, trace SignalBinding -> TwinModel/TwinVariable/Instrumentation.Signal/BindingRole/BindingStatus.</summary>
public sealed record TraceModelVariableToSignalQuery(string TwinCode) : IQuery<IReadOnlyList<ModelVariableSignalTraceDto>>;
