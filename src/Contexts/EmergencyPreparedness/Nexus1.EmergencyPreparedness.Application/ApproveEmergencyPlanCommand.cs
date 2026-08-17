using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Application;

/// <summary>EmergencyPlan's defining behavior (ADR-025): creates a new plan against its PlanStatusId.</summary>
public sealed record ApproveEmergencyPlanCommand(
    string Code, string Name, int PlanStatusId, int SiteId, int OwnerUserId, int? PlantId = null,
    int CurrentRevisionNumber = 0, DateTime? EffectiveFromUtc = null, DateTime? EffectiveToUtc = null,
    string? Description = null)
    : ICommand<int>;
