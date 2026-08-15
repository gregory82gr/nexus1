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
using Nexus1.ReactorFleet.Application;
using Nexus1.ReactorFleet.Infrastructure;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Composition root only — no business logic (dependency law, Nexus1.ArchitectureTests).
var alarmManagementConnectionString = builder.Configuration.GetConnectionString("AlarmManagementDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:AlarmManagementDb configuration.");
var auditConnectionString = builder.Configuration.GetConnectionString("AuditDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:AuditDb configuration.");
var complianceConnectionString = builder.Configuration.GetConnectionString("ComplianceDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:ComplianceDb configuration.");

builder.Services.AddBuildingBlocksApplication();

var rabbitMqOptions = new RabbitMqOptions(
    builder.Configuration["RabbitMq:HostName"] ?? "localhost",
    int.Parse(builder.Configuration["RabbitMq:Port"] ?? "5672"),
    builder.Configuration["RabbitMq:UserName"] ?? "guest",
    builder.Configuration["RabbitMq:Password"] ?? "guest",
    builder.Configuration["RabbitMq:VirtualHost"] ?? "/");
builder.Services.AddNexusMessaging(rabbitMqOptions);

builder.Services.AddReactorFleetApplication();
builder.Services.AddReactorFleetInfrastructure(alarmManagementConnectionString);

builder.Services.AddAlarmManagementApplication();
builder.Services.AddAlarmManagementInfrastructure(alarmManagementConnectionString);

builder.Services.AddAuditInfrastructure(auditConnectionString);

builder.Services.AddComplianceInfrastructure(complianceConnectionString);

builder.Services
    .AddHealthChecks()
    .AddCheck<DbContextHealthCheck<ReactorFleetDbContext>>("reactorfleet-db")
    .AddCheck<DbContextHealthCheck<AlarmManagementDbContext>>("alarmmanagement-db")
    .AddCheck<DbContextHealthCheck<AuditDbContext>>("audit-db")
    .AddCheck<DbContextHealthCheck<ComplianceDbContext>>("compliance-db");

var app = builder.Build();

// Liveness: process is up, no dependency checks (ADR-007).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Readiness: can this host actually reach every database it composes.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

app.Run();
