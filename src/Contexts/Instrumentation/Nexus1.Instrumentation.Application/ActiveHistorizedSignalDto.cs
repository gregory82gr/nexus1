namespace Nexus1.Instrumentation.Application;

/// <summary>Atlas C.5.8 query 1 projection: Tag, Name, category code, unit symbol, retention code.</summary>
public sealed record ActiveHistorizedSignalDto(string Tag, string Name, string CategoryCode, string UnitSymbol, string RetentionCode);
