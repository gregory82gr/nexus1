using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// A physical muster destination (ADR-025) — the atlas's own C.14.1 purpose
/// text names this exact linkage ("where people must assemble",
/// radiological awareness). RadiationZoneId is a real SQL FOREIGN KEY to
/// RadiationMonitoring.RadiationZone via the
/// RadiationMonitoringRadiationZoneReference shadow-entity technique,
/// nullable — not every assembly point sits inside a monitored zone.
///
/// SiteId/PlantId are deliberately downgraded to plain passport ints, no
/// enforced FK — Organization.Site/Plant live in OrganizationDb, a
/// different physical database than AlarmManagementDb (ADR-025).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only.
/// </summary>
public sealed class AssemblyPoint : Entity<AssemblyPointId>, IAggregateRoot
{
    private AssemblyPoint(
        AssemblyPointId id, string code, string name, int siteId, int? plantId, int? radiationZoneId,
        int? maxOccupancy, bool isIndoor, decimal? latitude, decimal? longitude, string? description)
        : base(id)
    {
        Code = code;
        Name = name;
        SiteId = siteId;
        PlantId = plantId;
        RadiationZoneId = radiationZoneId;
        MaxOccupancy = maxOccupancy;
        IsIndoor = isIndoor;
        Latitude = latitude;
        Longitude = longitude;
        Description = description;
    }

    public string Code { get; }

    public string Name { get; }

    /// <summary>Passport-only — Organization.Site lives in OrganizationDb (ADR-025).</summary>
    public int SiteId { get; }

    /// <summary>Passport-only — Organization.Plant lives in OrganizationDb (ADR-025).</summary>
    public int? PlantId { get; }

    /// <summary>Real FK to RadiationMonitoring.RadiationZone (ADR-025), nullable — not every assembly point sits inside a monitored zone.</summary>
    public int? RadiationZoneId { get; }

    public int? MaxOccupancy { get; }

    public bool IsIndoor { get; }

    public decimal? Latitude { get; }

    public decimal? Longitude { get; }

    public string? Description { get; }

    public static AssemblyPoint Create(
        AssemblyPointId id, string code, string name, int siteId, int? plantId = null,
        int? radiationZoneId = null, int? maxOccupancy = null, bool isIndoor = false, decimal? latitude = null,
        decimal? longitude = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("AssemblyPoint code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("AssemblyPoint name must not be empty.", nameof(name));
        }

        return new AssemblyPoint(
            id, code, name, siteId, plantId, radiationZoneId, maxOccupancy, isIndoor, latitude, longitude,
            description);
    }
}
