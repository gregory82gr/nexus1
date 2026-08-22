using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>
/// The join between an EvacuationRoute and a radiological zone it crosses
/// (ADR-025), atlas query 3's own subject. EvacuationRouteId is a real
/// internal FK, NOT NULL. RadiationZoneId is a real SQL FOREIGN KEY to
/// RadiationMonitoring.RadiationZone via the
/// RadiationMonitoringRadiationZoneReference shadow-entity technique, NOT
/// NULL, unique together with EvacuationRouteId (enforced at the EF
/// configuration layer).
///
/// No audit columns at all.
/// </summary>
public sealed class EvacuationRouteZone : Entity<EvacuationRouteZoneId>, IAggregateRoot
{
    private EvacuationRouteZone(
        EvacuationRouteZoneId id, EvacuationRouteId evacuationRouteId, int radiationZoneId,
        bool isAvoidIfAlarmed, string? notes)
        : base(id)
    {
        EvacuationRouteId = evacuationRouteId;
        RadiationZoneId = radiationZoneId;
        IsAvoidIfAlarmed = isAvoidIfAlarmed;
        Notes = notes;
    }

    public EvacuationRouteId EvacuationRouteId { get; }

    /// <summary>Real FK to RadiationMonitoring.RadiationZone (ADR-025), NOT NULL.</summary>
    public int RadiationZoneId { get; }

    public bool IsAvoidIfAlarmed { get; }

    public string? Notes { get; }

    public static EvacuationRouteZone Create(
        EvacuationRouteZoneId id, EvacuationRouteId evacuationRouteId, int radiationZoneId,
        bool isAvoidIfAlarmed = true, string? notes = null)
    {
        return new EvacuationRouteZone(id, evacuationRouteId, radiationZoneId, isAvoidIfAlarmed, notes);
    }
}
