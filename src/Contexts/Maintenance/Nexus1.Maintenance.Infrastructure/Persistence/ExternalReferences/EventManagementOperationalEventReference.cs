using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto EventManagement.OperationalEvent's
/// key column, used solely so EF Core can declare a genuine SQL FOREIGN KEY
/// from Maintenance.WorkOrder.OriginOperationalEventId to
/// EventManagement.OperationalEvent across bounded contexts that share
/// AlarmManagementDb (ADR-021, ADR-022's reconnection decision).
///
/// This is a follow-up to ADR-021's own original build: OriginOperationalEventId
/// was originally passport-only with no enforced FK at all, because
/// EventManagement (atlas C.8) did not exist anywhere in this project yet.
/// Now that it does (ADR-022) and shares this same physical database, the
/// reference becomes a real FK — same shadow-entity technique as
/// ReactorFleetUnitReference in this same folder, not a new pattern.
/// </summary>
internal sealed class EventManagementOperationalEventReference
{
    public long OperationalEventId { get; set; }

    /// <summary>Read-only projection of OperationalEvent.EventCode, needed by the reconnected GetWorkOrdersWithOriginQuery. Never written by this context.</summary>
    public string EventCode { get; set; } = string.Empty;
}

internal sealed class EventManagementOperationalEventReferenceConfiguration
    : IEntityTypeConfiguration<EventManagementOperationalEventReference>
{
    public void Configure(EntityTypeBuilder<EventManagementOperationalEventReference> builder)
    {
        builder.ToTable("OperationalEvent", "EventManagement", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.OperationalEventId);
        builder.Property(x => x.OperationalEventId).HasColumnName("OperationalEventId").ValueGeneratedNever();
        builder.Property(x => x.EventCode).HasColumnName("EventCode").HasMaxLength(80);
    }
}
