using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

/// <summary>No audit columns in the real DDL for this table — lean, append-only link (ADR-022). AlarmEventId is a real FK via the first-ever AlarmManagement shadow reference.</summary>
public sealed class EventAlarmLinkConfiguration : IEntityTypeConfiguration<EventAlarmLink>
{
    public void Configure(EntityTypeBuilder<EventAlarmLink> builder)
    {
        builder.ToTable("EventAlarmLink", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_EventAlarmLink");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EventAlarmLinkId(value))
            .HasColumnName("EventAlarmLinkId")
            .ValueGeneratedNever();

        builder.Property(x => x.OperationalEventId)
            .HasConversion(id => id.Value, value => new OperationalEventId(value))
            .HasColumnName("OperationalEventId")
            .IsRequired();

        builder.Property(x => x.AlarmEventId).HasColumnName("AlarmEventId").IsRequired();
        builder.Property(x => x.LinkRole).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => new { x.OperationalEventId, x.AlarmEventId }).IsUnique().HasDatabaseName("UQ_EventManagement_EventAlarmLink");

        builder.HasOne<OperationalEvent>()
            .WithMany()
            .HasForeignKey(x => x.OperationalEventId)
            .HasConstraintName("FK_EventManagement_EventAlarmLink_OperationalEvent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AlarmManagementAlarmEventReference>()
            .WithMany()
            .HasForeignKey(x => x.AlarmEventId)
            .HasConstraintName("FK_EventManagement_EventAlarmLink_AlarmEvent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
