using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

/// <summary>Atlas C.3.8 query 3, verbatim.</summary>
public sealed record GetLatestStaffingGapsQuery(int StaffingScenarioId) : IQuery<IReadOnlyList<StaffingScenarioGapDto>>;
