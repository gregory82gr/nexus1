using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

/// <summary>Atlas C.6.8 query 1, verbatim: every active (IsDeleted = 0) twin and the unit it mirrors, model type, status and fidelity.</summary>
public sealed record GetActiveTwinsForFleetQuery : IQuery<IReadOnlyList<ActiveTwinDto>>;
