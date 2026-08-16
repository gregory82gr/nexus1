using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Infrastructure;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.Audit.Infrastructure;
using Nexus1.Audit.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.Compliance.Infrastructure;
using Nexus1.Compliance.Infrastructure.Persistence;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Infrastructure;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Infrastructure;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.Organization.Application;
using Nexus1.Organization.Infrastructure;
using Nexus1.Organization.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Application;
using Nexus1.ReactorFleet.Infrastructure;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.Reporting.Infrastructure;
using Nexus1.Reporting.Infrastructure.Persistence;
using Nexus1.Security.Application;
using Nexus1.Security.Infrastructure;
using Nexus1.Security.Infrastructure.Persistence;
using Nexus1.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Composition root only — no business logic (dependency law, Nexus1.ArchitectureTests).
var alarmManagementConnectionString = builder.Configuration.GetConnectionString("AlarmManagementDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:AlarmManagementDb configuration.");
var auditConnectionString = builder.Configuration.GetConnectionString("AuditDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:AuditDb configuration.");
var complianceConnectionString = builder.Configuration.GetConnectionString("ComplianceDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:ComplianceDb configuration.");
var reportingConnectionString = builder.Configuration.GetConnectionString("ReportingDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:ReportingDb configuration.");
var securityConnectionString = builder.Configuration.GetConnectionString("SecurityDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:SecurityDb configuration.");
var organizationConnectionString = builder.Configuration.GetConnectionString("OrganizationDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:OrganizationDb configuration.");

builder.Services.AddBuildingBlocksApplication();

var otlpEndpoint = new Uri(builder.Configuration["Otel:OtlpEndpoint"] ?? "http://localhost:4317");
builder.Services.AddNexusObservability(new NexusObservabilityOptions("Nexus1.ModularRuntime", otlpEndpoint));

var rabbitMqOptions = new RabbitMqOptions(
    builder.Configuration["RabbitMq:HostName"] ?? "localhost",
    int.Parse(builder.Configuration["RabbitMq:Port"] ?? "5672"),
    builder.Configuration["RabbitMq:UserName"] ?? "guest",
    builder.Configuration["RabbitMq:Password"] ?? "guest",
    builder.Configuration["RabbitMq:VirtualHost"] ?? "/");
builder.Services.AddNexusMessaging(rabbitMqOptions);

builder.Services.AddReactorFleetApplication();
builder.Services.AddReactorFleetInfrastructure(alarmManagementConnectionString);

// Shares AlarmManagementDb (ADR-015, following ADR-006's precedent) — CorePlatform
// is protected foundation composed in-process here, not independently deployed.
builder.Services.AddCorePlatformApplication();
builder.Services.AddCorePlatformInfrastructure(alarmManagementConnectionString);

builder.Services.AddAlarmManagementApplication();
builder.Services.AddAlarmManagementInfrastructure(alarmManagementConnectionString);

// Shares AlarmManagementDb (ADR-019) — every external reference in Instrumentation's
// fifteen-table scope points at ReactorFleet.Unit or CorePlatform.EngineeringUnit, both
// already in this same physical database; no PII/credential sensitivity pulls toward
// isolation, so it is composed here alongside them rather than getting its own database.
builder.Services.AddInstrumentationApplication();
builder.Services.AddInstrumentationInfrastructure(alarmManagementConnectionString);

builder.Services.AddAuditInfrastructure(auditConnectionString);

builder.Services.AddComplianceInfrastructure(complianceConnectionString);

builder.Services.AddReportingInfrastructure(reportingConnectionString);

// Own physical database, SecurityDb (ADR-016) — not shared with the other
// contexts above, unlike CorePlatform/ReactorFleet: ApplicationUser holds
// credential-adjacent columns, a data-sensitivity reason to isolate
// independent of deployment topology.
builder.Services.AddSecurityApplication();
builder.Services.AddSecurityInfrastructure(securityConnectionString);

// Own physical database, OrganizationDb (ADR-017) — not shared with the other
// contexts above: Person holds real PII (GivenName, FamilyName, DisplayName,
// WorkEmail, WorkPhone), the same class of data-sensitivity reason ADR-016
// used to isolate Security, independent of deployment topology.
builder.Services.AddOrganizationApplication();
builder.Services.AddOrganizationInfrastructure(organizationConnectionString);

builder.Services
    .AddHealthChecks()
    .AddCheck<DbContextHealthCheck<ReactorFleetDbContext>>("reactorfleet-db")
    .AddCheck<DbContextHealthCheck<CorePlatformDbContext>>("coreplatform-db")
    .AddCheck<DbContextHealthCheck<InstrumentationDbContext>>("instrumentation-db")
    .AddCheck<DbContextHealthCheck<AlarmManagementDbContext>>("alarmmanagement-db")
    .AddCheck<DbContextHealthCheck<AuditDbContext>>("audit-db")
    .AddCheck<DbContextHealthCheck<ComplianceDbContext>>("compliance-db")
    .AddCheck<DbContextHealthCheck<ReportingDbContext>>("reporting-db")
    .AddCheck<DbContextHealthCheck<SecurityDbContext>>("security-db")
    .AddCheck<DbContextHealthCheck<OrganizationDbContext>>("organization-db");

var app = builder.Build();

// Liveness: process is up, no dependency checks (ADR-007).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Readiness: can this host actually reach every database it composes.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

app.Run();
