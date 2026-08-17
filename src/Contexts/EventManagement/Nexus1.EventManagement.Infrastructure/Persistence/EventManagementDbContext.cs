using Microsoft.EntityFrameworkCore;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-022, following
/// ADR-006/ADR-015/ADR-019/ADR-020/ADR-021's precedent) but keeps its own
/// migration history and only owns EventManagement.* tables. The
/// ExternalReferences types (ReactorFleetUnitReference,
/// AlarmManagementAlarmEventReference, AlarmManagementAlarmFloodReference)
/// are also part of this model — configured via IEntityTypeConfiguration and
/// picked up by ApplyConfigurationsFromAssembly below — but are excluded
/// from this context's migrations; they exist only so FK relationships to
/// tables owned by other contexts can be declared.
/// </summary>
public sealed class EventManagementDbContext(DbContextOptions<EventManagementDbContext> options) : DbContext(options)
{
    public DbSet<EventType> EventTypes => Set<EventType>();

    public DbSet<EventStatus> EventStatuses => Set<EventStatus>();

    public DbSet<EventSeverity> EventSeverities => Set<EventSeverity>();

    public DbSet<EventSourceType> EventSourceTypes => Set<EventSourceType>();

    public DbSet<EventTimelineEntryType> EventTimelineEntryTypes => Set<EventTimelineEntryType>();

    public DbSet<IncidentType> IncidentTypes => Set<IncidentType>();

    public DbSet<IncidentStatus> IncidentStatuses => Set<IncidentStatus>();

    public DbSet<IncidentActionType> IncidentActionTypes => Set<IncidentActionType>();

    public DbSet<IncidentActionStatus> IncidentActionStatuses => Set<IncidentActionStatus>();

    public DbSet<OperationalEvent> OperationalEvents => Set<OperationalEvent>();

    public DbSet<EventAlarmLink> EventAlarmLinks => Set<EventAlarmLink>();

    public DbSet<EventFloodLink> EventFloodLinks => Set<EventFloodLink>();

    public DbSet<EventTimelineEntry> EventTimelineEntries => Set<EventTimelineEntry>();

    public DbSet<Incident> Incidents => Set<Incident>();

    public DbSet<IncidentAction> IncidentActions => Set<IncidentAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventManagementDbContext).Assembly);
    }
}
