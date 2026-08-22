using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// The plan/revision spine's header (ADR-025) — the atlas's own design
/// statement realized directly: "The active plan points to the current
/// revision number." PlanStatusId is a real internal FK, NOT NULL.
///
/// SiteId/PlantId/OwnerUserId are deliberately downgraded to plain passport
/// ints, no enforced FK — Organization.Site/Plant live in OrganizationDb and
/// Security.ApplicationUser lives in SecurityDb, both different physical
/// databases than AlarmManagementDb (ADR-025).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only, same treatment as RadiationMonitoring.RadiationZone (ADR-024/025).
/// </summary>
public sealed class EmergencyPlan : Entity<EmergencyPlanId>, IAggregateRoot
{
    private EmergencyPlan(
        EmergencyPlanId id, string code, string name, PlanStatusId planStatusId, int siteId, int? plantId,
        int ownerUserId, int currentRevisionNumber, DateTime? effectiveFromUtc, DateTime? effectiveToUtc,
        string? description)
        : base(id)
    {
        Code = code;
        Name = name;
        PlanStatusId = planStatusId;
        SiteId = siteId;
        PlantId = plantId;
        OwnerUserId = ownerUserId;
        CurrentRevisionNumber = currentRevisionNumber;
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        Description = description;
    }

    public string Code { get; }

    public string Name { get; }

    public PlanStatusId PlanStatusId { get; }

    /// <summary>Passport-only — Organization.Site lives in OrganizationDb, a different physical database (ADR-025).</summary>
    public int SiteId { get; }

    /// <summary>Passport-only — Organization.Plant lives in OrganizationDb (ADR-025).</summary>
    public int? PlantId { get; }

    /// <summary>Passport-only — Security.ApplicationUser lives in SecurityDb (ADR-025).</summary>
    public int OwnerUserId { get; }

    public int CurrentRevisionNumber { get; }

    public DateTime? EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public string? Description { get; }

    public static EmergencyPlan Create(
        EmergencyPlanId id, string code, string name, PlanStatusId planStatusId, int siteId, int ownerUserId,
        int? plantId = null, int currentRevisionNumber = 0, DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EmergencyPlan code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EmergencyPlan name must not be empty.", nameof(name));
        }

        return new EmergencyPlan(
            id, code, name, planStatusId, siteId, plantId, ownerUserId, currentRevisionNumber, effectiveFromUtc,
            effectiveToUtc, description);
    }
}
