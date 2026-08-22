using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.EmergencyPreparedness.Domain;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

/// <summary>
/// Shared seed: one CorePlatform.EngineeringUnit row, the three
/// RadiationMonitoring lookup rows a RadiationZone needs plus one
/// RadiationZone row (the sector's two real cross-context FK targets,
/// ADR-025), the eight EmergencyPreparedness lookup rows, and one
/// EmergencyPlan/EmergencyPlanRevision/Exercise/ExerciseObservation/
/// AssemblyPoint/EvacuationRoute/EvacuationRouteZone/EmergencyResource/
/// ResourceReadinessCheck graph. Every component test below builds on this
/// same shape so the six Application operations are exercised against a
/// realistic, FK-consistent graph rather than isolated rows — copying
/// RadiationMonitoringSeedHelper's own precedent pattern exactly.
/// </summary>
internal static class EmergencyPreparednessSeedHelper
{
    public const int SiteId = 100;
    public const string EngineeringUnitSymbol = "units";
    public const string RadiationZoneCode = "RZ-EP-001";
    public const string EmergencyPlanCode = "EP-001";
    public const string ExerciseCode = "EX-001";
    public const string AssemblyPointCode = "AP-001";
    public const string EvacuationRouteCode = "ER-001";
    public const string EmergencyResourceCode = "RES-001";

    public sealed record SeedResult(
        int EngineeringUnitId, int RadiationZoneId, int PlanStatusId, int RouteStatusOpenId,
        int ResourceTypeId, int ReadinessStatusId, int ExerciseTypeId, int ExerciseStatusId,
        int ObservationSeverityId, int ResourceStatusId, int EmergencyPlanId, int EmergencyPlanRevisionId,
        int ExerciseId, int ExerciseObservationId, int AssemblyPointId, int EvacuationRouteId,
        int EvacuationRouteZoneId, int EmergencyResourceId, long ResourceReadinessCheckId);

    public static async Task<SeedResult> SeedCoreAsync(
        CorePlatformDbContext corePlatformContext, RadiationMonitoringDbContext radiationMonitoringContext,
        EmergencyPreparednessDbContext emergencyPreparednessContext, DateTime nowUtc)
    {
        var engineeringUnit = EngineeringUnit.Create(
            new EngineeringUnitId(1), EngineeringUnitSymbol, "Generic Unit", EngineeringQuantityType.RadiationDoseRate, nowUtc);
        await corePlatformContext.EngineeringUnits.AddAsync(engineeringUnit);
        await corePlatformContext.SaveChangesAsync();

        var radiationZoneType = RadiationZoneType.Create(new RadiationZoneTypeId(1), "CONTROLLED", "Controlled Area", nowUtc);
        var radiationZoneStatus = RadiationZoneStatus.Create(new RadiationZoneStatusId(1), "ACTIVE", "Active", nowUtc);
        var radiationAreaClassification = RadiationAreaClassification.Create(new RadiationAreaClassificationId(1), "LOW", "Low Radiation Area", nowUtc);
        await radiationMonitoringContext.RadiationZoneTypes.AddAsync(radiationZoneType);
        await radiationMonitoringContext.RadiationZoneStatuses.AddAsync(radiationZoneStatus);
        await radiationMonitoringContext.RadiationAreaClassifications.AddAsync(radiationAreaClassification);
        await radiationMonitoringContext.SaveChangesAsync();

        var radiationZone = RadiationZone.Create(
            new RadiationZoneId(1), radiationZoneType.Id, radiationZoneStatus.Id, radiationAreaClassification.Id,
            RadiationZoneCode, "Muster Route Zone 1");
        await radiationMonitoringContext.RadiationZones.AddAsync(radiationZone);
        await radiationMonitoringContext.SaveChangesAsync();

        var planStatus = PlanStatus.Create(new PlanStatusId(1), "APPROVED", "Approved", nowUtc);
        var routeStatusOpen = RouteStatus.Create(new RouteStatusId(1), "OPEN", "Open", nowUtc);
        var resourceType = ResourceType.Create(new ResourceTypeId(1), "EQUIPMENT", "Equipment", nowUtc);
        var readinessStatus = ReadinessStatus.Create(new ReadinessStatusId(1), "READY", "Ready", nowUtc);
        var exerciseType = ExerciseType.Create(new ExerciseTypeId(1), "DRILL", "Drill", nowUtc);
        var exerciseStatus = ExerciseStatus.Create(new ExerciseStatusId(1), "COMPLETED", "Completed", nowUtc);
        var observationSeverity = ObservationSeverity.Create(new ObservationSeverityId(1), "MAJOR", "Major", nowUtc);
        var resourceStatus = ResourceStatus.Create(new ResourceStatusId(1), "AVAILABLE", "Available", nowUtc);

        await emergencyPreparednessContext.PlanStatuses.AddAsync(planStatus);
        await emergencyPreparednessContext.RouteStatuses.AddAsync(routeStatusOpen);
        await emergencyPreparednessContext.ResourceTypes.AddAsync(resourceType);
        await emergencyPreparednessContext.ReadinessStatuses.AddAsync(readinessStatus);
        await emergencyPreparednessContext.ExerciseTypes.AddAsync(exerciseType);
        await emergencyPreparednessContext.ExerciseStatuses.AddAsync(exerciseStatus);
        await emergencyPreparednessContext.ObservationSeverities.AddAsync(observationSeverity);
        await emergencyPreparednessContext.ResourceStatuses.AddAsync(resourceStatus);
        await emergencyPreparednessContext.SaveChangesAsync();

        const int ownerUserId = 501;
        var emergencyPlan = EmergencyPlan.Create(
            new EmergencyPlanId(1), EmergencyPlanCode, "Site Emergency Plan", planStatus.Id, SiteId, ownerUserId,
            currentRevisionNumber: 1);
        await emergencyPreparednessContext.EmergencyPlans.AddAsync(emergencyPlan);
        await emergencyPreparednessContext.SaveChangesAsync();

        var emergencyPlanRevision = EmergencyPlanRevision.Create(
            new EmergencyPlanRevisionId(1), emergencyPlan.Id, 1, "Initial Revision", planStatus.Id, ownerUserId, nowUtc);
        await emergencyPreparednessContext.EmergencyPlanRevisions.AddAsync(emergencyPlanRevision);
        await emergencyPreparednessContext.SaveChangesAsync();

        const int coordinatorUserId = 502;
        var exercise = Exercise.Create(
            new ExerciseId(1), ExerciseCode, "Site-Wide Fire Drill", exerciseType.Id, exerciseStatus.Id, SiteId,
            nowUtc.AddDays(-7), nowUtc.AddDays(-7).AddHours(3), coordinatorUserId,
            actualStartUtc: nowUtc.AddDays(-7), actualEndUtc: nowUtc.AddDays(-7).AddHours(3));
        await emergencyPreparednessContext.Exercises.AddAsync(exercise);
        await emergencyPreparednessContext.SaveChangesAsync();

        const int observedByUserId = 503;
        var exerciseObservation = ExerciseObservation.Create(
            new ExerciseObservationId(1), exercise.Id, observationSeverity.Id, observedByUserId,
            nowUtc.AddDays(-7).AddHours(1), "Muster count did not match roster within five minutes.",
            correctiveActionRequired: true);
        await emergencyPreparednessContext.ExerciseObservations.AddAsync(exerciseObservation);
        await emergencyPreparednessContext.SaveChangesAsync();

        var assemblyPoint = AssemblyPoint.Create(
            new AssemblyPointId(1), AssemblyPointCode, "North Parking Lot", SiteId,
            radiationZoneId: radiationZone.Id.Value, maxOccupancy: 250);
        await emergencyPreparednessContext.AssemblyPoints.AddAsync(assemblyPoint);
        await emergencyPreparednessContext.SaveChangesAsync();

        var evacuationRoute = EvacuationRoute.Create(
            new EvacuationRouteId(1), EvacuationRouteCode, "Turbine Hall Evacuation Route", SiteId,
            assemblyPoint.Id, routeStatusOpen.Id, "Turbine Hall Main Floor", estimatedMinutes: 6);
        await emergencyPreparednessContext.EvacuationRoutes.AddAsync(evacuationRoute);
        await emergencyPreparednessContext.SaveChangesAsync();

        var evacuationRouteZone = EvacuationRouteZone.Create(
            new EvacuationRouteZoneId(1), evacuationRoute.Id, radiationZone.Id.Value);
        await emergencyPreparednessContext.EvacuationRouteZones.AddAsync(evacuationRouteZone);
        await emergencyPreparednessContext.SaveChangesAsync();

        var emergencyResource = EmergencyResource.Create(
            new EmergencyResourceId(1), EmergencyResourceCode, "Portable Air Compressor", resourceType.Id,
            resourceStatus.Id, SiteId, quantityOnHand: 2m, engineeringUnitId: engineeringUnit.Id.Value);
        await emergencyPreparednessContext.EmergencyResources.AddAsync(emergencyResource);
        await emergencyPreparednessContext.SaveChangesAsync();

        const int checkedByUserId = 504;
        var resourceReadinessCheck = ResourceReadinessCheck.Create(
            new ResourceReadinessCheckId(1), emergencyResource.Id, readinessStatus.Id, nowUtc, checkedByUserId,
            "Compressor started and ran for ten minutes without fault.");
        await emergencyPreparednessContext.ResourceReadinessChecks.AddAsync(resourceReadinessCheck);
        await emergencyPreparednessContext.SaveChangesAsync();

        return new SeedResult(
            engineeringUnit.Id.Value, radiationZone.Id.Value, planStatus.Id.Value, routeStatusOpen.Id.Value,
            resourceType.Id.Value, readinessStatus.Id.Value, exerciseType.Id.Value, exerciseStatus.Id.Value,
            observationSeverity.Id.Value, resourceStatus.Id.Value, emergencyPlan.Id.Value,
            emergencyPlanRevision.Id.Value, exercise.Id.Value, exerciseObservation.Id.Value, assemblyPoint.Id.Value,
            evacuationRoute.Id.Value, evacuationRouteZone.Id.Value, emergencyResource.Id.Value,
            resourceReadinessCheck.Id.Value);
    }
}
