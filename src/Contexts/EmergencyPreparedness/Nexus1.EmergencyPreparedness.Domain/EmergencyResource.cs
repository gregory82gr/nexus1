using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// A governed emergency-response asset (ADR-025) — mirrors
/// RadiationMonitoring.RadiationMonitor's own shape from the immediately
/// prior sector, a governed asset with a periodic readiness/calibration
/// check trail (ResourceReadinessCheck). ResourceTypeId/ResourceStatusId
/// are real internal FKs, NOT NULL. EngineeringUnitId is a real SQL FOREIGN
/// KEY to CorePlatform.EngineeringUnit via the
/// CorePlatformEngineeringUnitReference shadow-entity technique, nullable.
///
/// SiteId/PlantId/OwnerTeamId are deliberately downgraded to plain passport
/// ints, no enforced FK — Organization.Site/Plant/Team live in
/// OrganizationDb, a different physical database than AlarmManagementDb
/// (ADR-025).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only.
/// </summary>
public sealed class EmergencyResource : Entity<EmergencyResourceId>, IAggregateRoot
{
    private EmergencyResource(
        EmergencyResourceId id, string code, string name, ResourceTypeId resourceTypeId,
        ResourceStatusId resourceStatusId, int siteId, int? plantId, int? ownerTeamId, decimal? quantityOnHand,
        int? engineeringUnitId, string? locationText)
        : base(id)
    {
        Code = code;
        Name = name;
        ResourceTypeId = resourceTypeId;
        ResourceStatusId = resourceStatusId;
        SiteId = siteId;
        PlantId = plantId;
        OwnerTeamId = ownerTeamId;
        QuantityOnHand = quantityOnHand;
        EngineeringUnitId = engineeringUnitId;
        LocationText = locationText;
    }

    public string Code { get; }

    public string Name { get; }

    public ResourceTypeId ResourceTypeId { get; }

    public ResourceStatusId ResourceStatusId { get; }

    /// <summary>Passport-only — Organization.Site lives in OrganizationDb (ADR-025).</summary>
    public int SiteId { get; }

    /// <summary>Passport-only — Organization.Plant lives in OrganizationDb (ADR-025).</summary>
    public int? PlantId { get; }

    /// <summary>Passport-only — Organization.Team lives in OrganizationDb (ADR-025).</summary>
    public int? OwnerTeamId { get; }

    public decimal? QuantityOnHand { get; }

    /// <summary>Real FK to CorePlatform.EngineeringUnit (ADR-025), nullable.</summary>
    public int? EngineeringUnitId { get; }

    public string? LocationText { get; }

    public static EmergencyResource Create(
        EmergencyResourceId id, string code, string name, ResourceTypeId resourceTypeId,
        ResourceStatusId resourceStatusId, int siteId, int? plantId = null, int? ownerTeamId = null,
        decimal? quantityOnHand = null, int? engineeringUnitId = null, string? locationText = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EmergencyResource code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EmergencyResource name must not be empty.", nameof(name));
        }

        return new EmergencyResource(
            id, code, name, resourceTypeId, resourceStatusId, siteId, plantId, ownerTeamId, quantityOnHand,
            engineeringUnitId, locationText);
    }
}
