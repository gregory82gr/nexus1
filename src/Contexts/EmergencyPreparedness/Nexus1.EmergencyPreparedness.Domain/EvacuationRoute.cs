using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// A physical evacuation path terminating at an AssemblyPoint (ADR-025).
/// AssemblyPointId and RouteStatusId are real internal FKs, NOT NULL —
/// AssemblyPointId is FK-integrity-forced into scope by this NOT NULL
/// column (ADR-025's own scope reasoning).
///
/// SiteId/PlantId are deliberately downgraded to plain passport ints, no
/// enforced FK — Organization.Site/Plant live in OrganizationDb (ADR-025).
///
/// Full audit shape is NOT modeled in Domain at all — EF shadow properties
/// only.
/// </summary>
public sealed class EvacuationRoute : Entity<EvacuationRouteId>, IAggregateRoot
{
    private EvacuationRoute(
        EvacuationRouteId id, string code, string name, int siteId, int? plantId, AssemblyPointId assemblyPointId,
        RouteStatusId routeStatusId, string fromLocation, int? estimatedMinutes, string? routeGeometryJson,
        string? notes)
        : base(id)
    {
        Code = code;
        Name = name;
        SiteId = siteId;
        PlantId = plantId;
        AssemblyPointId = assemblyPointId;
        RouteStatusId = routeStatusId;
        FromLocation = fromLocation;
        EstimatedMinutes = estimatedMinutes;
        RouteGeometryJson = routeGeometryJson;
        Notes = notes;
    }

    public string Code { get; }

    public string Name { get; }

    /// <summary>Passport-only — Organization.Site lives in OrganizationDb (ADR-025).</summary>
    public int SiteId { get; }

    /// <summary>Passport-only — Organization.Plant lives in OrganizationDb (ADR-025).</summary>
    public int? PlantId { get; }

    public AssemblyPointId AssemblyPointId { get; }

    public RouteStatusId RouteStatusId { get; }

    public string FromLocation { get; }

    public int? EstimatedMinutes { get; }

    public string? RouteGeometryJson { get; }

    public string? Notes { get; }

    public static EvacuationRoute Create(
        EvacuationRouteId id, string code, string name, int siteId, AssemblyPointId assemblyPointId,
        RouteStatusId routeStatusId, string fromLocation, int? plantId = null, int? estimatedMinutes = null,
        string? routeGeometryJson = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EvacuationRoute code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EvacuationRoute name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(fromLocation))
        {
            throw new ArgumentException("EvacuationRoute from-location must not be empty.", nameof(fromLocation));
        }

        return new EvacuationRoute(
            id, code, name, siteId, plantId, assemblyPointId, routeStatusId, fromLocation, estimatedMinutes,
            routeGeometryJson, notes);
    }
}
