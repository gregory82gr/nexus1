using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

/// <summary>Matches the atlas's own C.1.8 verification query verbatim: "All active units of measure used for instrumentation."</summary>
public sealed record GetActiveEngineeringUnitsQuery : IQuery<IReadOnlyList<EngineeringUnitDto>>;
