namespace Nexus1.Organization.Application;

public sealed record StaffingScenarioGapDto(int PositionId, int RequiredCount, int AvailableCount, int GapCount, string? Notes);
