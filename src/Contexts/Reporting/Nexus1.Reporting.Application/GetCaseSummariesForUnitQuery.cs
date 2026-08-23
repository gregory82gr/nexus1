using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Reporting.Application;

public sealed record GetCaseSummariesForUnitQuery(int UnitId) : IQuery<IReadOnlyList<CaseSummaryDto>>;
