using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

/// <summary>No audit columns in the real DDL for this table — lean, append-only link (ADR-022). AlarmFloodId is a real FK via the second first-ever AlarmManagement shadow reference.</summary>
public sealed class EventFloodLinkConfiguration : IEntityTypeConfiguration<EventFloodLink>
{
    public void Configure(EntityTypeBuilder<EventFloodLink> builder)
    {
        builder.ToTable("EventFloodLink", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_EventFloodLink");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EventFloodLinkId(value))
            .HasColumnName("EventFloodLinkId")
            .ValueGeneratedNever();

        builder.Property(x => x.OperationalEventId)
            .HasConversion(id => id.Value, value => new OperationalEventId(value))
            .HasColumnName("OperationalEventId")
            .IsRequired();

        builder.Property(x => x.AlarmFloodId).HasColumnName("AlarmFloodId").IsRequired();
        builder.Property(x => x.LinkRole).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => new { x.OperationalEventId, x.AlarmFloodId }).IsUnique().HasDatabaseName("UQ_EventManagement_EventFloodLink");

        builder.HasOne<OperationalEvent>()
            .WithMany()
            .HasForeignKey(x => x.OperationalEventId)
            .HasConstraintName("FK_EventManagement_EventFloodLink_OperationalEvent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AlarmManagementAlarmFloodReference>()
            .WithMany()
            .HasForeignKey(x => x.AlarmFloodId)
            .HasConstraintName("FK_EventManagement_EventFloodLink_AlarmFlood")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
