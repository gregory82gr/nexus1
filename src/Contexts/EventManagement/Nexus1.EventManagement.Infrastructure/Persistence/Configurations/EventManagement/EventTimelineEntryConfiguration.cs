using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

/// <summary>
/// No audit columns in the real DDL for this table — lean, append-only
/// chronology (ADR-022). EnteredByUserId carries a real FK to
/// Security.ApplicationUser in the atlas's own single-database DDL, but
/// SecurityDb is a different physical database than this context's
/// AlarmManagementDb home, so it stays passport-only with no HasOne/FK.
/// </summary>
public sealed class EventTimelineEntryConfiguration : IEntityTypeConfiguration<EventTimelineEntry>
{
    public void Configure(EntityTypeBuilder<EventTimelineEntry> builder)
    {
        builder.ToTable("EventTimelineEntry", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_EventTimelineEntry");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EventTimelineEntryId(value))
            .HasColumnName("EventTimelineEntryId")
            .ValueGeneratedNever();

        builder.Property(x => x.OperationalEventId)
            .HasConversion(id => id.Value, value => new OperationalEventId(value))
            .HasColumnName("OperationalEventId")
            .IsRequired();

        builder.Property(x => x.EventTimelineEntryTypeId)
            .HasConversion(id => id.Value, value => new EventTimelineEntryTypeId(value))
            .HasColumnName("EventTimelineEntryTypeId")
            .IsRequired();

        builder.Property(x => x.EntryAtUtc).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(2000);
        builder.Property(x => x.EnteredByUserId).HasColumnName("EnteredByUserId");
        builder.Property(x => x.SourceReference).HasMaxLength(200);

        builder.HasOne<OperationalEvent>()
            .WithMany()
            .HasForeignKey(x => x.OperationalEventId)
            .HasConstraintName("FK_EventManagement_EventTimelineEntry_OperationalEvent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EventTimelineEntryType>()
            .WithMany()
            .HasForeignKey(x => x.EventTimelineEntryTypeId)
            .HasConstraintName("FK_EventManagement_EventTimelineEntry_Type")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
