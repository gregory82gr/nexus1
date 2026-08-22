namespace Nexus1.Organization.Application;

/// <summary>One position's required/available counts for a staffing scenario evaluation — GapCount is computed by the domain, never accepted here.</summary>
public sealed record StaffingGapRequest(int PositionId, int RequiredCount, int AvailableCount, string? Notes = null);
