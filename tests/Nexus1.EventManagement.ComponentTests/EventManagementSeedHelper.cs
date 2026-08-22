using Nexus1.AlarmManagement.Domain;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

/// <summary>
/// Shared seed: one ReactorFleet.Unit row, one AlarmManagement.AlarmDefinition/
/// AlarmEvent/AlarmFlood set (the real cross-context FK targets, ADR-022), and
/// the nine EventManagement lookup rows. Every component test below builds on
/// this same shape so the seven Application operations are exercised against
/// a realistic, FK-consistent graph rather than isolated rows — copying
/// MaintenanceSeedHelper's own precedent pattern exactly (ADR-021/ADR-022).
/// </summary>
internal static class EventManagementSeedHelper
{
    public const string UnitCode = "NX1-U1";
    public const string EventCode = "EVT-2026-0418";

    public sealed record SeedResult(
        int UnitId, long AlarmEventId, long AlarmFloodId, int EventTypeId, int EventStatusId, int EventSeverityId,
        int EventSourceTypeId, int EventTimelineEntryTypeId, int IncidentTypeId, int IncidentStatusId,
        int IncidentActionTypeId, int IncidentActionStatusOpenId, int IncidentActionStatusCompletedId,
        int IncidentActionStatusVerifiedId, int IncidentActionStatusCancelledId);

    public static async Task<SeedResult> SeedCoreAsync(
        ReactorFleetDbContext reactorFleetContext, AlarmManagementDbContext alarmManagementContext,
        EventManagementDbContext eventManagementContext, DateTime nowUtc)
    {
        var unit = Unit.Create(new Nexus1.ReactorFleet.Domain.UnitId(1), UnitCode, "Unit 1");
        await reactorFleetContext.Units.AddAsync(unit);
        await reactorFleetContext.SaveChangesAsync();

        var alarmDefinition = AlarmDefinition.Create(
            new AlarmDefinitionId(1), new Nexus1.AlarmManagement.Domain.UnitId(unit.Id.Value), "FW-FLOW-LOW",
            "Feedwater Flow Low", AlarmSeverity.High, 100m);
        await alarmManagementContext.AlarmDefinitions.AddAsync(alarmDefinition);
        await alarmManagementContext.SaveChangesAsync();

        var alarmEvent = AlarmEvent.Raise(
            new AlarmEventId(1), alarmDefinition.Id, new Nexus1.AlarmManagement.Domain.UnitId(unit.Id.Value),
            AlarmSeverity.High, nowUtc, sourceValue: 85m, thresholdValue: 100m, message: "FW-FLOW-LOW breached: 85 < 100.");
        await alarmManagementContext.AlarmEvents.AddAsync(alarmEvent);
        await alarmManagementContext.SaveChangesAsync();

        var alarmFlood = AlarmFlood.Detect(new AlarmFloodId(1), new Nexus1.AlarmManagement.Domain.UnitId(unit.Id.Value), nowUtc);
        await alarmManagementContext.AlarmFloods.AddAsync(alarmFlood);
        await alarmManagementContext.SaveChangesAsync();

        var eventType = EventType.Create(new EventTypeId(1), "INCIDENT", "Incident", nowUtc);
        var eventStatus = EventStatus.Create(new EventStatusId(1), "NEW", "New", nowUtc);
        var eventSeverity = EventSeverity.Create(new EventSeverityId(1), "MAJOR", "Major", nowUtc);
        var eventSourceType = EventSourceType.Create(new EventSourceTypeId(1), "ALARM_FLOOD", "Alarm Flood", nowUtc);
        var eventTimelineEntryType = EventTimelineEntryType.Create(new EventTimelineEntryTypeId(1), "DETECTED", "Detected", nowUtc);
        var incidentType = IncidentType.Create(new IncidentTypeId(1), "OPERATIONAL", "Operational", nowUtc);
        var incidentStatus = IncidentStatus.Create(new IncidentStatusId(1), "OPENED", "Opened", nowUtc);
        var incidentActionType = IncidentActionType.Create(new IncidentActionTypeId(1), "CORRECTIVE", "Corrective", nowUtc);
        var incidentActionStatusOpen = IncidentActionStatus.Create(new IncidentActionStatusId(1), "ASSIGNED", "Assigned", nowUtc);
        var incidentActionStatusCompleted = IncidentActionStatus.Create(new IncidentActionStatusId(2), "COMPLETED", "Completed", nowUtc);
        var incidentActionStatusVerified = IncidentActionStatus.Create(new IncidentActionStatusId(3), "VERIFIED", "Verified", nowUtc);
        var incidentActionStatusCancelled = IncidentActionStatus.Create(new IncidentActionStatusId(4), "CANCELLED", "Cancelled", nowUtc);

        await eventManagementContext.EventTypes.AddAsync(eventType);
        await eventManagementContext.EventStatuses.AddAsync(eventStatus);
        await eventManagementContext.EventSeverities.AddAsync(eventSeverity);
        await eventManagementContext.EventSourceTypes.AddAsync(eventSourceType);
        await eventManagementContext.EventTimelineEntryTypes.AddAsync(eventTimelineEntryType);
        await eventManagementContext.IncidentTypes.AddAsync(incidentType);
        await eventManagementContext.IncidentStatuses.AddAsync(incidentStatus);
        await eventManagementContext.IncidentActionTypes.AddAsync(incidentActionType);
        await eventManagementContext.IncidentActionStatuses.AddAsync(incidentActionStatusOpen);
        await eventManagementContext.IncidentActionStatuses.AddAsync(incidentActionStatusCompleted);
        await eventManagementContext.IncidentActionStatuses.AddAsync(incidentActionStatusVerified);
        await eventManagementContext.IncidentActionStatuses.AddAsync(incidentActionStatusCancelled);
        await eventManagementContext.SaveChangesAsync();

        return new SeedResult(
            unit.Id.Value, alarmEvent.Id.Value, alarmFlood.Id.Value, eventType.Id.Value, eventStatus.Id.Value,
            eventSeverity.Id.Value, eventSourceType.Id.Value, eventTimelineEntryType.Id.Value, incidentType.Id.Value,
            incidentStatus.Id.Value, incidentActionType.Id.Value, incidentActionStatusOpen.Id.Value,
            incidentActionStatusCompleted.Id.Value, incidentActionStatusVerified.Id.Value, incidentActionStatusCancelled.Id.Value);
    }
}
