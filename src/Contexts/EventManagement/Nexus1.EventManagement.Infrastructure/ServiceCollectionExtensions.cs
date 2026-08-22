using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-022 — EventManagement shares that physical database, own schema, own migration history, joining ReactorFleet/CorePlatform/AlarmManagement/Instrumentation/DigitalTwin/Maintenance).</summary>
    public static IServiceCollection AddEventManagementInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<EventManagementDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_EventManagement")));

        services.AddScoped<IRepository<EventType, EventTypeId>, EfRepository<EventType, EventTypeId>>();
        services.AddScoped<IRepository<EventStatus, EventStatusId>, EfRepository<EventStatus, EventStatusId>>();
        services.AddScoped<IRepository<EventSeverity, EventSeverityId>, EfRepository<EventSeverity, EventSeverityId>>();
        services.AddScoped<IRepository<EventSourceType, EventSourceTypeId>, EfRepository<EventSourceType, EventSourceTypeId>>();
        services.AddScoped<IRepository<EventTimelineEntryType, EventTimelineEntryTypeId>, EfRepository<EventTimelineEntryType, EventTimelineEntryTypeId>>();
        services.AddScoped<IRepository<IncidentType, IncidentTypeId>, EfRepository<IncidentType, IncidentTypeId>>();
        services.AddScoped<IRepository<IncidentStatus, IncidentStatusId>, EfRepository<IncidentStatus, IncidentStatusId>>();
        services.AddScoped<IRepository<IncidentActionType, IncidentActionTypeId>, EfRepository<IncidentActionType, IncidentActionTypeId>>();
        services.AddScoped<IRepository<IncidentActionStatus, IncidentActionStatusId>, EfRepository<IncidentActionStatus, IncidentActionStatusId>>();

        services.AddScoped<IRepository<OperationalEvent, OperationalEventId>, EfRepository<OperationalEvent, OperationalEventId>>();
        services.AddScoped<IRepository<EventAlarmLink, EventAlarmLinkId>, EfRepository<EventAlarmLink, EventAlarmLinkId>>();
        services.AddScoped<IRepository<EventFloodLink, EventFloodLinkId>, EfRepository<EventFloodLink, EventFloodLinkId>>();
        services.AddScoped<IRepository<EventTimelineEntry, EventTimelineEntryId>, EfRepository<EventTimelineEntry, EventTimelineEntryId>>();
        services.AddScoped<IRepository<Incident, IncidentId>, EfRepository<Incident, IncidentId>>();
        services.AddScoped<IRepository<IncidentAction, IncidentActionId>, EfRepository<IncidentAction, IncidentActionId>>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IEventWithAlarmsAndFloodFinder, EfEventWithAlarmsAndFloodFinder>();
        services.AddScoped<IEventTimelineFinder, EfEventTimelineFinder>();
        services.AddScoped<IOpenIncidentActionsFinder, EfOpenIncidentActionsFinder>();
        services.AddScoped<IIncidentExistenceFinder, EfIncidentExistenceFinder>();

        return services;
    }
}
