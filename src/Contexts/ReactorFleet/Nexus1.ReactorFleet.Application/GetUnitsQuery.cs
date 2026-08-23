using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReactorFleet.Application;

public sealed record GetUnitsQuery : IQuery<IReadOnlyList<UnitSummaryDto>>;
