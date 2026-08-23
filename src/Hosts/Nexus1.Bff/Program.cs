using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Infrastructure;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Infrastructure;
using Nexus1.DigitalTwin.Infrastructure.Persistence;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Infrastructure;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Application;
using Nexus1.ReactorFleet.Infrastructure;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.Reporting.Application;
using Nexus1.Reporting.Infrastructure;
using Nexus1.Reporting.Infrastructure.Persistence;
using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Infrastructure;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.Organization.Application;
using Nexus1.Organization.Infrastructure;
using Nexus1.Organization.Infrastructure.Persistence;
using Nexus1.Robotics.Application;
using Nexus1.Robotics.Infrastructure;
using Nexus1.Robotics.Infrastructure.Persistence;
using Nexus1.Security.Application;
using Nexus1.Security.Infrastructure;
using Nexus1.Security.Infrastructure.Persistence;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Infrastructure;
using Nexus1.Maintenance.Infrastructure.Persistence;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Infrastructure;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.Audit.Application;
using Nexus1.Audit.Infrastructure;
using Nexus1.Audit.Infrastructure.Persistence;
using Nexus1.Compliance.Application;
using Nexus1.Compliance.Infrastructure;
using Nexus1.Compliance.Infrastructure.Persistence;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Infrastructure;
using Nexus1.EventManagement.Infrastructure.Persistence;
using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Infrastructure;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;
using Nexus1.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Composition root only — no business logic (dependency law, Nexus1.ArchitectureTests).
// ADR-030: walking-skeleton vertical slice, ReactorFleet only. Every context
// resolved in-process here the same way Nexus1.ModularRuntime does today — no
// HTTP hop for in-process contexts. RootCause is untouched (still reached the
// way ADR-001 already established) since no endpoint in this slice needs it.
var alarmManagementConnectionString = builder.Configuration.GetConnectionString("AlarmManagementDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:AlarmManagementDb configuration.");
// Falls back to alarmManagementConnectionString when no override is configured — same physical
// database as every other context above by default. An explicit ConnectionStrings:RadiationMonitoringDb
// entry lets one context be pointed elsewhere independently (e.g. to prove the Overview endpoint's
// partial-failure behavior with a genuinely broken connection for one section only, ADR-030 evidence)
// without touching the shared connection string the other five contexts still use.
var radiationMonitoringConnectionString = builder.Configuration.GetConnectionString("RadiationMonitoringDb")
    ?? alarmManagementConnectionString;
// Reporting owns its own physical database, ReportingDb (ADR-012) — not
// shared with AlarmManagementDb, unlike ReactorFleet/AlarmManagement/
// DigitalTwin/RadiationMonitoring above.
var reportingConnectionString = builder.Configuration.GetConnectionString("ReportingDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:ReportingDb configuration.");
// Organization owns its own physical database, OrganizationDb (ADR-017) —
// Person carries real PII, a data-sensitivity reason to isolate independent
// of deployment topology.
var organizationConnectionString = builder.Configuration.GetConnectionString("OrganizationDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:OrganizationDb configuration.");
// Security owns its own physical database, SecurityDb (ADR-016) — credential-
// adjacent columns, a data-sensitivity reason to isolate independent of
// deployment topology.
var securityConnectionString = builder.Configuration.GetConnectionString("SecurityDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:SecurityDb configuration.");
// Audit owns its own physical database, AuditDb (ADR-010) — a data-ownership
// requirement (no FK to RootCauseDb, ch.34 34-AH), not a deployment-topology one.
var auditConnectionString = builder.Configuration.GetConnectionString("AuditDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:AuditDb configuration.");
// Compliance owns its own physical database, ComplianceDb (ADR-011) — same
// data-ownership reasoning as AuditDb, not deployment topology.
var complianceConnectionString = builder.Configuration.GetConnectionString("ComplianceDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:ComplianceDb configuration.");

// Dev-testing convenience only — NOT a new architectural layer (see
// src/Hosts/Nexus1.Bff/README.md). Config-driven subset of contexts to
// compose, for evidence-gathering runs that don't need all eleven and
// would otherwise pay their full memory/startup cost. Unset or empty (the
// production/full-integration default) means "compose everything," so
// every one of the ten already-proven slices' behavior is unchanged when
// this isn't used. Bound from ConnectionStrings-style configuration —
// appsettings.json's "BffContexts": { "Enabled": ["Maintenance"] }, or the
// equivalent environment variables (BffContexts__Enabled__0=ReactorFleet,
// BffContexts__Enabled__1=Maintenance, ...).
var enabledContexts = builder.Configuration.GetSection("BffContexts:Enabled").Get<string[]>();
bool IsContextEnabled(string contextName) =>
    enabledContexts is null || enabledContexts.Length == 0 || enabledContexts.Contains(contextName, StringComparer.OrdinalIgnoreCase);

builder.Services.AddBuildingBlocksApplication();

if (IsContextEnabled("ReactorFleet"))
{
    builder.Services.AddReactorFleetApplication();
    builder.Services.AddReactorFleetInfrastructure(alarmManagementConnectionString);
}

if (IsContextEnabled("AlarmManagement"))
{
    builder.Services.AddAlarmManagementApplication();
    // enableOutboxRelay: false — the BFF is a read/write API surface for this
    // slice, not a second outbox-relay/metrics process for AlarmManagement's
    // messaging backbone; Nexus1.ModularRuntime already owns that job. The BFF
    // registers neither AddNexusMessaging nor AddNexusObservability (ADR-030),
    // so composing the relay here would crash host startup with an unresolved
    // IBrokerPublisher/OutboxMetricState — confirmed directly (see evidence).
    builder.Services.AddAlarmManagementInfrastructure(alarmManagementConnectionString, enableOutboxRelay: false);
}

if (IsContextEnabled("DigitalTwin"))
{
    // DigitalTwin shares AlarmManagementDb too (ADR-020). No enableOutboxRelay-style
    // opt-out needed here — checked directly: AddDigitalTwinInfrastructure registers
    // zero hosted services (this Phase 2 sector has no messaging/outbox at all,
    // per ADR-027), so there was no AlarmManagement-style startup surprise to guard against.
    builder.Services.AddDigitalTwinApplication();
    builder.Services.AddDigitalTwinInfrastructure(alarmManagementConnectionString);
}

if (IsContextEnabled("RadiationMonitoring"))
{
    // RadiationMonitoring shares AlarmManagementDb too (ADR-024). Same check as
    // DigitalTwin: AddRadiationMonitoringInfrastructure registers zero hosted
    // services (confirmed directly, not assumed from the Phase 2/ADR-027
    // pattern alone) — no opt-out parameter needed here either.
    builder.Services.AddRadiationMonitoringApplication();
    builder.Services.AddRadiationMonitoringInfrastructure(radiationMonitoringConnectionString);
}

if (IsContextEnabled("Reporting"))
{
    // Reporting is Phase 1, with a full messaging backbone (consumer + retry
    // dispatcher), unlike DigitalTwin/RadiationMonitoring's Phase 2 no-messaging
    // precedent — confirmed by reading ReportingConsumerBackgroundService (needs
    // RabbitMqConnectionManager/RabbitMqOptions), ReportingProjectionMessageHandler
    // (needs NexusRuntimeMetrics), and RetryDispatcher (needs IBrokerPublisher)
    // directly, all unregistered here. enableMessagingConsumer: false skips all
    // three, same AlarmManagement-style opt-out pattern. Reporting also had zero
    // Application layer before this task — Nexus1.Reporting.Application is new,
    // added following the same IQuery/IQueryHandler/Finder convention as every
    // other context (see evidence for the full explanation).
    builder.Services.AddReportingApplication();
    builder.Services.AddReportingInfrastructure(reportingConnectionString, enableMessagingConsumer: false);
}

if (IsContextEnabled("Robotics"))
{
    // Robotics shares AlarmManagementDb too (ADR-023). Same check as DigitalTwin/
    // RadiationMonitoring: AddRoboticsInfrastructure registers zero hosted
    // services (confirmed directly, not assumed) — no opt-out parameter needed.
    builder.Services.AddRoboticsApplication();
    builder.Services.AddRoboticsInfrastructure(alarmManagementConnectionString);
}

if (IsContextEnabled("Instrumentation"))
{
    // Instrumentation shares AlarmManagementDb too (ADR-019). Same check as
    // DigitalTwin/RadiationMonitoring/Robotics: AddInstrumentationInfrastructure
    // registers zero hosted services (confirmed directly) — no opt-out needed.
    builder.Services.AddInstrumentationApplication();
    builder.Services.AddInstrumentationInfrastructure(alarmManagementConnectionString);
}

if (IsContextEnabled("Organization"))
{
    // Organization is Phase 2 (ADR-017). Same check as DigitalTwin/RadiationMonitoring/
    // Robotics/Instrumentation: AddOrganizationInfrastructure registers zero hosted
    // services (confirmed directly) — no opt-out needed.
    builder.Services.AddOrganizationApplication();
    builder.Services.AddOrganizationInfrastructure(organizationConnectionString);
}

if (IsContextEnabled("Security"))
{
    // Security (ADR-016). Same check as every context so far:
    // AddSecurityInfrastructure registers zero hosted services (confirmed
    // directly) — no opt-out needed. Zero new Application-layer code needed
    // either — GetEffectivePermissionsForUserQueryHandler already existed and
    // is wired in as-is.
    builder.Services.AddSecurityApplication();
    builder.Services.AddSecurityInfrastructure(securityConnectionString);
}

if (IsContextEnabled("Maintenance"))
{
    // Maintenance shares AlarmManagementDb too (ADR-021). Same check as
    // DigitalTwin/RadiationMonitoring/Robotics/Instrumentation/Organization:
    // AddMaintenanceInfrastructure registers zero hosted services (confirmed
    // directly) — no opt-out needed.
    builder.Services.AddMaintenanceApplication();
    builder.Services.AddMaintenanceInfrastructure(alarmManagementConnectionString);
}

if (IsContextEnabled("CorePlatform"))
{
    // CorePlatform shares AlarmManagementDb too (ADR-015). Same check as
    // every context so far: AddCorePlatformInfrastructure registers zero
    // hosted services (confirmed directly) — no opt-out needed. Zero new
    // Application-layer code needed either — GetCurrentDeploymentVersionsQueryHandler
    // and GetActiveEngineeringUnitsQueryHandler already existed and are
    // wired in as-is.
    builder.Services.AddCorePlatformApplication();
    builder.Services.AddCorePlatformInfrastructure(alarmManagementConnectionString);
}

if (IsContextEnabled("Audit"))
{
    // Audit had NO Application layer at all before this slice — built one
    // from scratch (same situation Reporting was in). enableMessagingConsumer:
    // false — read by directly reading AuditConsumerBackgroundService/
    // AuditVerdictMessageHandler/RetryDispatcher's constructors (same rigor as
    // Reporting/AlarmManagement, not assumed from either precedent): all three
    // need RabbitMqConnectionManager/RabbitMqOptions/NexusRuntimeMetrics/
    // IBrokerPublisher, none of which the BFF registers.
    builder.Services.AddAuditApplication();
    builder.Services.AddAuditInfrastructure(auditConnectionString, enableMessagingConsumer: false);
}

if (IsContextEnabled("Compliance"))
{
    // Compliance had NO Application layer at all before this slice either —
    // built one from scratch, same shape as Audit. enableMessagingConsumer:
    // false — confirmed by reading each constructor directly, same rigor as
    // Audit/Reporting/AlarmManagement: ComplianceConsumerBackgroundService/
    // ComplianceVerdictMessageHandler/RetryDispatcher all need
    // RabbitMqConnectionManager/RabbitMqOptions/NexusRuntimeMetrics/
    // IBrokerPublisher, none of which the BFF registers.
    builder.Services.AddComplianceApplication();
    builder.Services.AddComplianceInfrastructure(complianceConnectionString, enableMessagingConsumer: false);
}

if (IsContextEnabled("EventManagement"))
{
    // Shares AlarmManagementDb (ADR-022 — joining ReactorFleet/CorePlatform/
    // AlarmManagement/Instrumentation/DigitalTwin/Maintenance). Confirmed by
    // reading AddEventManagementInfrastructure directly: zero
    // AddHostedService<...>() calls — no opt-out parameter needed, unlike
    // Audit/Compliance/Reporting/AlarmManagement. Zero new Application-layer
    // code needed either — all three query handlers already existed
    // (atlas C.8.5.2's own three named queries) and are wired in as-is.
    //
    // Named gap, investigated against the Angular companion book's full
    // 39-screen sitemap before building anything here: no screen backs this
    // domain. "Alarms & Events" (Ch. 23) is entirely AlarmManagement's own
    // alarm feed; "Incident Analysis"/"Root Cause Graph" (Ch. 29) are
    // entirely RootCause's fault-tree synthesis — neither touches Incident/
    // IncidentAction/EventTimelineEntry anywhere in the book's text. This is
    // real, rich, unit-scoped domain data (incidents, corrective actions,
    // event timelines) exposed honestly as backend-only — not forced into a
    // screen shape it doesn't fit.
    builder.Services.AddEventManagementApplication();
    builder.Services.AddEventManagementInfrastructure(alarmManagementConnectionString);
}

if (IsContextEnabled("EmergencyPreparedness"))
{
    // Shares AlarmManagementDb (ADR-025). Re-confirmed by reading
    // AddEmergencyPreparednessInfrastructure directly right before this
    // wiring (not just relying on the earlier investigation pass): zero
    // AddHostedService<...>() calls — no opt-out parameter needed. Zero new
    // Application-layer code needed either — all four query handlers
    // already existed (the atlas's own four named verification queries) and
    // are wired in as-is.
    //
    // Named gap, investigated against the Angular companion book's full
    // 39-screen sitemap before building anything here (exhaustive search
    // for Emergency/Evacuation/Assembly Point/Exercise/Readiness/Drill —
    // nothing matched): no screen backs this domain. Real, useful domain
    // data (plan status, resource readiness, exercise/drill history,
    // evacuation routes) exposed honestly as backend-only, same treatment
    // as EventManagement.
    //
    // Site-scoped, not unit-scoped — the first context in this BFF with
    // this granularity. Only GetSiteActivePlansQuery takes a real scoping
    // parameter (SiteId); the other three queries are fleet-wide with no
    // parameter at all (even the readiness dashboard, whose DTO carries
    // SiteId per row but is queried unscoped, grouped across every site).
    builder.Services.AddEmergencyPreparednessApplication();
    builder.Services.AddEmergencyPreparednessInfrastructure(alarmManagementConnectionString);
}

var healthChecksBuilder = builder.Services.AddHealthChecks();
if (IsContextEnabled("ReactorFleet"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<ReactorFleetDbContext>>("reactorfleet-db");
}

if (IsContextEnabled("AlarmManagement"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<AlarmManagementDbContext>>("alarmmanagement-db");
}

if (IsContextEnabled("DigitalTwin"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<DigitalTwinDbContext>>("digitaltwin-db");
}

if (IsContextEnabled("RadiationMonitoring"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<RadiationMonitoringDbContext>>("radiationmonitoring-db");
}

if (IsContextEnabled("Reporting"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<ReportingDbContext>>("reporting-db");
}

if (IsContextEnabled("Robotics"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<RoboticsDbContext>>("robotics-db");
}

if (IsContextEnabled("Instrumentation"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<InstrumentationDbContext>>("instrumentation-db");
}

if (IsContextEnabled("Organization"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<OrganizationDbContext>>("organization-db");
}

if (IsContextEnabled("Security"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<SecurityDbContext>>("security-db");
}

if (IsContextEnabled("Maintenance"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<MaintenanceDbContext>>("maintenance-db");
}

if (IsContextEnabled("CorePlatform"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<CorePlatformDbContext>>("coreplatform-db");
}

if (IsContextEnabled("Audit"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<AuditDbContext>>("audit-db");
}

if (IsContextEnabled("Compliance"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<ComplianceDbContext>>("compliance-db");
}

if (IsContextEnabled("EventManagement"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<EventManagementDbContext>>("eventmanagement-db");
}

if (IsContextEnabled("EmergencyPreparedness"))
{
    healthChecksBuilder.AddCheck<DbContextHealthCheck<EmergencyPreparednessDbContext>>("emergencypreparedness-db");
}

var app = builder.Build();

// Liveness: process is up, no dependency checks (ADR-007 precedent).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Readiness: can this host actually reach every database it composes.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

// Fleet-overview screen: minimal summary per unit (ADR-030).
app.MapGet("/api/v1/reactor-fleet/units", async ([FromServices] GetUnitsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitsQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

// Unit-detail screen: summary fields plus recent power history (ADR-030).
app.MapGet("/api/v1/reactor-fleet/units/{id:int}", async (int id, [FromServices] GetUnitByIdQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitByIdQuery(id), cancellationToken);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(new { error = result.Error });
});

// Alarm-monitoring screen: fleet-wide active alarms (ADR-030 follow-up slice).
app.MapGet("/api/v1/alarm-management/alarms/active", async ([FromServices] GetActiveAlarmsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetActiveAlarmsQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

// Acknowledge an active alarm — same AcknowledgeAlarmCommandHandler Nexus1.ModularRuntime
// uses; no outbox/messaging side effect here (AcknowledgeAlarmCommandHandler never
// touches IOutboxWriter — confirmed by reading it, not assumed), so nothing about
// this write path depends on the relay this host deliberately doesn't run.
app.MapPost("/api/v1/alarm-management/alarms/{id:long}/acknowledge", async (
    long id, AcknowledgeAlarmRequest request, [FromServices] AcknowledgeAlarmCommandHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new AcknowledgeAlarmCommand(id, request.AcknowledgedByUserId), cancellationToken);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
});

// Plant 3D View screen: per-unit twin state (ADR-030 follow-up slice). Does
// NOT include divergence/sync-drift data — a named gap, see
// IActiveTwinFinder.GetActiveTwinsForUnitAsync's doc comment for why.
app.MapGet("/api/v1/digital-twin/units/{id:int}", async (int id, [FromServices] GetUnitTwinStateQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitTwinStateQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Radiation & Safety screen: per-unit ambient monitor readings and zone
// classification (ADR-030 follow-up slice). Does NOT include personnel dose
// data (DoseAlert/PersonDoseReading/Dosimeter) — a named gap, not an
// oversight: dose in this domain model is tracked per PERSON, never per
// unit. See UnitRadiationSafetyDto's doc comment for the full explanation.
app.MapGet("/api/v1/radiation-monitoring/units/{id:int}", async (int id, [FromServices] GetUnitRadiationSafetyQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitRadiationSafetyQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Trends & History screen: per-unit root-cause case history (ADR-030
// follow-up slice). This is investigation-case history (RootCause analyses
// opened from an alarm flood, eventually closed with a verdict), NOT a
// generic sensor time-series — Reporting's real domain model has no such
// concept. Shaped honestly around what Reporting actually is, not what the
// screen name might suggest. See ICaseSummaryFinder's doc comment.
app.MapGet("/api/v1/reporting/units/{id:int}", async (int id, [FromServices] GetCaseSummariesForUnitQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetCaseSummariesForUnitQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Robotics Fleet Overview / Mission Readiness screens: per-unit robot status
// + health, and mission summaries (ADR-030 follow-up slice). Mission-summary
// level only — does NOT include per-mission readiness-item detail or event
// timeline (GetBlockingReadinessFailuresQuery/GetMissionTimelineQuery, both
// scoped by a specific MissionId) — a named boundary, see UnitMissionDto's
// doc comment.
app.MapGet("/api/v1/robotics/units/{id:int}", async (int id, [FromServices] GetUnitRoboticsOverviewQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitRoboticsOverviewQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Reactor sub-screens (Core, Control Rods, Kinetics, Neutronics, Coolant/TH,
// Steam Generators — six of the book's seven Reactor screens; the seventh,
// Model Analysis, is a separate real grouping, below) — ONE endpoint, not six.
// Instrumentation's domain model has no separate entity per subsystem; every
// one of those six screens is just a filtered view over the same generic
// Signal/Measurement rows. See UnitSignalReadingDto's doc comment.
app.MapGet("/api/v1/instrumentation/units/{id:int}/signals", async (int id, [FromServices] GetUnitSignalReadingsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitSignalReadingsQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Model Analysis screen (the 7th Reactor screen) — Instrumentation's own real
// "verification" concept: open signal-quality/data-trust incidents for a
// unit's telemetry (SignalQualityEvent, a genuinely separate aggregate with
// its own open/close lifecycle, not a re-slice of the signals endpoint
// above). This is verification of telemetry trustworthiness, not physics-
// model verification (that's DigitalTwin's divergence data, a separate gap
// already recorded there).
app.MapGet("/api/v1/instrumentation/units/{id:int}/signal-quality", async (int id, [FromServices] GetUnitSignalQualityEventsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitSignalQualityEventsQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Plant Overview / Dashboard screen (ADR-030 follow-up) — the first
// cross-context composition. Composes four already-existing per-unit
// queries (ReactorFleet unit identity, AlarmManagement's original
// per-unit active-alarms query — not the fleet-wide one from that slice's
// own endpoint — RadiationMonitoring's safety data, Instrumentation's
// signal readings), reusing each handler exactly as built for its own
// slice; no query logic duplicated here.
//
// Concurrency: all four calls are started immediately (an async method
// call runs synchronously up to its first await, so all four are already
// in flight before the first `await` statement below even executes) and
// awaited together via Task.WhenAll — not sequentially. Each hits a
// genuinely separate DbContext instance (ReactorFleetDbContext,
// AlarmManagementDbContext, RadiationMonitoringDbContext,
// InstrumentationDbContext are four distinct scoped services), so running
// them concurrently is safe — no single DbContext instance is ever used
// from more than one place at a time. See evidence for timing proof.
//
// Partial failure: each call is wrapped so a thrown exception in one
// section does not fail the other three or the endpoint as a whole — the
// failing section comes back null and its error message is recorded in
// Errors, keyed by section name. A dashboard with three working sections
// and one marked-failed is more useful than a 500 for the whole screen.
// The one case that IS a whole-endpoint failure: ReactorFleet's own query
// succeeding but reporting no such unit (Result.IsFailure, not an
// exception) — there is nothing to overview, so that's a genuine 404, not
// a partial response. A ReactorFleet query that THROWS is different from
// a confirmed "no such unit" and is treated as a partial failure like any
// other section, not a 404 — a broken query is not proof the unit doesn't
// exist.
app.MapGet("/api/v1/overview/units/{id:int}", async (
    int id,
    [FromServices] GetUnitByIdQueryHandler unitHandler,
    [FromServices] GetActiveAlarmsForUnitQueryHandler alarmsHandler,
    [FromServices] GetUnitRadiationSafetyQueryHandler radiationHandler,
    [FromServices] GetUnitSignalReadingsQueryHandler signalsHandler,
    CancellationToken cancellationToken) =>
{
    var unitTask = SafeCallAsync(async () =>
    {
        var result = await unitHandler.Handle(new GetUnitByIdQuery(id), cancellationToken);
        return result.IsSuccess ? result.Value : null;
    });
    var alarmsTask = SafeCallAsync(async () =>
    {
        var result = await alarmsHandler.Handle(new GetActiveAlarmsForUnitQuery(id), cancellationToken);
        return result.Value;
    });
    var radiationTask = SafeCallAsync(async () =>
    {
        var result = await radiationHandler.Handle(new GetUnitRadiationSafetyQuery(id), cancellationToken);
        return result.Value;
    });
    var signalsTask = SafeCallAsync(async () =>
    {
        var result = await signalsHandler.Handle(new GetUnitSignalReadingsQuery(id), cancellationToken);
        return result.Value;
    });

    await Task.WhenAll(unitTask, alarmsTask, radiationTask, signalsTask);

    var (unit, unitError) = await unitTask;
    var (alarms, alarmsError) = await alarmsTask;
    var (radiation, radiationError) = await radiationTask;
    var (signals, signalsError) = await signalsTask;

    if (unit is null && unitError is null)
    {
        return Results.NotFound(new { error = $"Unit {id} does not exist." });
    }

    var errors = new Dictionary<string, string>();
    if (unitError is not null)
    {
        errors["unit"] = unitError;
    }

    if (alarmsError is not null)
    {
        errors["activeAlarms"] = alarmsError;
    }

    if (radiationError is not null)
    {
        errors["radiation"] = radiationError;
    }

    if (signalsError is not null)
    {
        errors["signals"] = signalsError;
    }

    return Results.Ok(new OverviewDto(id, unit, alarms, radiation, signals, errors));
});

// Personnel screen (ADR-030 follow-up slice) — scoped to a Department, NOT
// a ReactorFleet unit: there is no connection at all between ReactorFleet.Unit
// and Organization's hierarchy in this codebase, not even passport-only.
// Plant.cs's own doc comment records this explicitly as deferred wiring
// (ADR-017) never actually performed. A per-unit personnel roster endpoint
// would have nothing real to query, so this is shaped around what
// Organization's own hierarchy actually supports: Department -> DepartmentAssignment
// -> Person. ApplicationUserId is surfaced as the raw passport int (ADR-028) —
// Organization only knows whether a person has a linked Security login, never
// any detail about it; resolving that further is Security's own job (see the
// Security slice's own evidence for what that context can and can't add).
app.MapGet("/api/v1/organization/departments/{id:int}/roster", async (int id, [FromServices] GetDepartmentRosterQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetDepartmentRosterQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Named gap: this is NOT a "Zone Access" (physical/area access) endpoint —
// Security's domain model has no zone/physical-access concept anywhere.
// ApplicationUser/ApplicationRole/Permission/PermissionCategory are entirely
// application-level RBAC (atlas C.2.3/C.2.4: "alarm acknowledgement, report
// export, security administration" — software actions, not physical doors
// or areas). There is nothing in this schema a "Zone Access" screen could
// honestly show. What Security genuinely has — effective permissions for a
// user, already fully built (GetEffectivePermissionsForUserQueryHandler,
// zero new Application-layer code needed here) — is wired in instead, named
// for what it actually is. This is also the resolution point for the
// Organization slice's Person.ApplicationUserId passport reference: anyone
// holding that int (e.g. Jordan Chen, ApplicationUserId 42, from the
// Organization roster) resolves their own access through this endpoint, in
// Security's own context, not Organization's.
app.MapGet("/api/v1/security/users/{id:int}/permissions", async (int id, [FromServices] GetEffectivePermissionsForUserQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetEffectivePermissionsForUserQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Rod Inspection cluster (Inspection Overview, NDT Methods, Rod Type/Film —
// three of the book's screens; ADR-030 follow-up). ONE endpoint, not three:
// Maintenance's domain model has no rod-specific entity anywhere — Asset/
// AssetCondition are entirely generic (any maintainable equipment item,
// generic category/status/grade lookups). NDT Methods and Rod Type/Film
// have nothing to map to at all, not missing fields on an otherwise
// rod-shaped model — see UnitAssetConditionDto's doc comment.
app.MapGet("/api/v1/maintenance/units/{id:int}/assets", async (int id, [FromServices] GetUnitAssetConditionsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetUnitAssetConditionsQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

// Named gap: "Component Registry" is NOT an honest name for what CorePlatform
// actually models. CorePlatform has no physical-equipment/component entity
// anywhere — its own "components" (DeploymentVersion.ComponentName/ComponentType)
// are SOFTWARE deployment artifacts (Console, Schema, SeedData, ApiService,
// Worker, Documentation — atlas C.1.4.9's own check constraint), not plant
// equipment. What CorePlatform genuinely has is fleet-wide/global reference
// data, not per-unit: a registry of currently-deployed software components,
// and a registry of engineering units of measure. Both endpoints below wire
// in already-existing, already-built handlers as-is — zero new Application-
// layer code needed in CorePlatform for this slice.
app.MapGet("/api/v1/core-platform/deployment-versions", async ([FromServices] GetCurrentDeploymentVersionsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetCurrentDeploymentVersionsQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

// Genuine reference-data registry (units of measure — °C, %RTP, kPa, etc.),
// referenced across the whole platform (instrumentation signals, alarm
// thresholds, model variables, reports) instead of free-text symbols. Also
// fleet-wide/global, not per-unit — the same reasoning as deployment-versions
// above.
app.MapGet("/api/v1/core-platform/engineering-units", async ([FromServices] GetActiveEngineeringUnitsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetActiveEngineeringUnitsQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

// Named gap: Audit has no UnitId anywhere, and no general "who changed what
// record" system audit-trail concept — its entire domain is one append-only
// evidence ledger (AuditEvidenceRecord) keyed by SourceAnalysisId, a RootCause
// analysis. RootCause stays out-of-process (ADR-001), so this endpoint cannot
// resolve that id into a human-readable case name — it's surfaced as-is. The
// realistic scope this screen half can honestly offer is per-analysis, not
// per-unit or an unscoped fleet-wide dump (a ledger with no natural top-level
// listing key wouldn't be a sane "give me everything" endpoint either).
app.MapGet("/api/v1/audit/analyses/{analysisId:long}/evidence", async (long analysisId, [FromServices] GetAuditEvidenceBySourceAnalysisIdQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetAuditEvidenceBySourceAnalysisIdQuery(analysisId), cancellationToken);
    return Results.Ok(result.Value);
});

// Named gap: "Compliance status/findings" is not an honest description of
// what this endpoint can show today. ComplianceReviewState has exactly one
// member (Pending) and ComplianceReview exposes no method that ever
// transitions it — review assignment, findings, and a decision are the
// book's own named-future authority (ch.34 34-AL), not implemented in this
// codebase yet. Every real row this endpoint returns will read State:
// "Pending". Same per-analysis scoping as Audit, for the same reason
// (no UnitId anywhere; RootCause stays out-of-process per ADR-001).
// Backend-only, no console screen: investigated against the Angular
// companion book's full sitemap before building anything (see Program.cs's
// EventManagement composition comment above for the full reasoning). These
// three endpoints wire in EventManagement's three already-existing queries
// (atlas C.8.5.2) as-is, exposed honestly as real domain data with no
// current UI consumer rather than forced into a screen shape.
app.MapGet("/api/v1/event-management/events/{eventCode}", async (string eventCode, [FromServices] GetEventWithAlarmsAndFloodQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetEventWithAlarmsAndFloodQuery(eventCode), cancellationToken);
    return result.Value is null ? Results.NotFound() : Results.Ok(result.Value);
});

app.MapGet("/api/v1/event-management/events/{operationalEventId:long}/timeline", async (long operationalEventId, [FromServices] GetEventTimelineQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetEventTimelineQuery(operationalEventId), cancellationToken);
    return Results.Ok(result.Value);
});

app.MapGet("/api/v1/event-management/incident-actions/open", async ([FromServices] GetOpenIncidentActionsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetOpenIncidentActionsQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

// Backend-only, no console screen: investigated against the Angular
// companion book's full sitemap before building anything (see Program.cs's
// EmergencyPreparedness composition comment above for the full reasoning),
// same treatment as EventManagement. All four endpoints wire in
// EmergencyPreparedness's four already-existing queries as-is.
//
// Site-scoped, not unit-scoped (see composition comment above) — this is
// the only one of the four routed under a real scoping parameter; the
// other three are fleet-wide flat listings, named honestly rather than
// forced under a {siteId} or {unitId} prefix that doesn't fit their real
// query shape.
app.MapGet("/api/v1/emergency-preparedness/sites/{siteId:int}/plans", async (int siteId, [FromServices] GetSiteActivePlansQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetSiteActivePlansQuery(siteId), cancellationToken);
    return Results.Ok(result.Value);
});

app.MapGet("/api/v1/emergency-preparedness/exercises/corrective-observations", async ([FromServices] GetExercisesWithCorrectiveObservationsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetExercisesWithCorrectiveObservationsQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

// Includes the EvacuationRoute -> RadiationMonitoring.RadiationZone crossing
// join as-is (the atlas's own query 3, verbatim) — genuinely useful without
// overreaching: it is exactly what this query already computes, not a new
// join invented for this slice.
app.MapGet("/api/v1/emergency-preparedness/evacuation-routes/open-or-restricted", async ([FromServices] GetOpenOrRestrictedRoutesCrossingZonesQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetOpenOrRestrictedRoutesCrossingZonesQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

app.MapGet("/api/v1/emergency-preparedness/resource-readiness-dashboard", async ([FromServices] GetResourceReadinessDashboardQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetResourceReadinessDashboardQuery(), cancellationToken);
    return Results.Ok(result.Value);
});

app.Run();

/// <summary>Runs one section's call, converting a thrown exception into an error message rather than letting it fail the whole composed response.</summary>
static async Task<(T? Data, string? Error)> SafeCallAsync<T>(Func<Task<T?>> call)
{
    try
    {
        var data = await call();
        return (data, null);
    }
    catch (Exception ex)
    {
        return (default, ex.Message);
    }
}

internal sealed record AcknowledgeAlarmRequest(Guid AcknowledgedByUserId);
