namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>Atlas verification query 3, verbatim: open/restricted evacuation routes and the radiological zones they cross.</summary>
public sealed record RouteCrossingZoneDto(string RouteCode, string RouteStatus, string RadiationZoneCode, bool IsAvoidIfAlarmed);
