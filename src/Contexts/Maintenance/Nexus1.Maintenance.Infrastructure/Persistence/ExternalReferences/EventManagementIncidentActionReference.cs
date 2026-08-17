using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

/// <summary>
/// Local, read-only shadow mapping onto EventManagement.IncidentAction's
/// key column, used solely so EF Core can declare a genuine SQL FOREIGN KEY
/// from Maintenance.WorkOrder.OriginIncidentActionId to
/// EventManagement.IncidentAction across bounded contexts that share
/// AlarmManagementDb (ADR-021, ADR-022's reconnection decision).
///
/// Exposes Title, not "ActionCode" — the atlas's own C.9.5.2 query 3 SQL
/// literally selects "ia.ActionCode", but IncidentAction's real DDL (atlas
/// C.8.4.5) has no such column; its real display field is Title. Followed
/// the real schema, not the query text's own apparent inconsistency —
/// same "atlas DDL is the schema authority" discipline this project has
/// applied consistently since ADR-017's Organization.Unit/Department
/// finding.
/// </summary>
internal sealed class EventManagementIncidentActionReference
{
    public long IncidentActionId { get; set; }

    /// <summary>Read-only projection of IncidentAction.Title — see this type's own doc comment for why not "ActionCode". Never written by this context.</summary>
    public string Title { get; set; } = string.Empty;
}

internal sealed class EventManagementIncidentActionReferenceConfiguration
    : IEntityTypeConfiguration<EventManagementIncidentActionReference>
{
    public void Configure(EntityTypeBuilder<EventManagementIncidentActionReference> builder)
    {
        builder.ToTable("IncidentAction", "EventManagement", tb => tb.ExcludeFromMigrations());
        builder.HasKey(x => x.IncidentActionId);
        builder.Property(x => x.IncidentActionId).HasColumnName("IncidentActionId").ValueGeneratedNever();
        builder.Property(x => x.Title).HasColumnName("Title").HasMaxLength(250);
    }
}
