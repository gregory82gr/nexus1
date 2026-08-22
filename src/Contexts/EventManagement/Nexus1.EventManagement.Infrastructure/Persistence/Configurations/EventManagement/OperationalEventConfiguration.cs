using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

/// <summary>
/// OperationalEvent's atlas DDL (C.8.4.3) carries the full audit-column
/// shape (CreatedBy, ModifiedAtUtc, ModifiedBy, IsDeleted, RowVersion). Same
/// shadow-property treatment as Maintenance's AssetConfiguration: CreatedBy/
/// ModifiedBy are NVARCHAR(100) NOT NULL with NO SQL DEFAULT in the atlas, so
/// they are mapped as EF shadow properties with HasDefaultValueSql("N'system'")
/// — a DEFAULT CONSTRAINT the atlas's own text does not specify, added solely
/// so the NOT NULL constraint is satisfiable without inventing an audit-trail
/// feature this phase was never asked to build. IsDeleted is likewise mapped
/// as a shadow property (default 0) — unlike Asset/WorkOrder, no verification
/// query in this sector's scope filters on it, so it is not domain-modeled.
///
/// PlantId, ReportedByUserId, OwnerUserId, OwningPersonId carry real FK
/// constraints in the atlas's own single-database DDL, but Organization.Plant/
/// Organization.Person live in OrganizationDb and Security.ApplicationUser
/// lives in SecurityDb — different physical databases than this context's
/// AlarmManagementDb home, so per this codebase's own convention (ADR-022)
/// they stay passport-only plain columns with no HasOne/FK declared.
/// </summary>
public sealed class OperationalEventConfiguration : IEntityTypeConfiguration<OperationalEvent>
{
    public void Configure(EntityTypeBuilder<OperationalEvent> builder)
    {
        builder.ToTable("OperationalEvent", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_OperationalEvent");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new OperationalEventId(value))
            .HasColumnName("OperationalEventId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();
        builder.Property(x => x.PlantId).HasColumnName("PlantId");

        builder.Property(x => x.EventTypeId)
            .HasConversion(id => id.Value, value => new EventTypeId(value))
            .HasColumnName("EventTypeId")
            .IsRequired();

        builder.Property(x => x.EventStatusId)
            .HasConversion(id => id.Value, value => new EventStatusId(value))
            .HasColumnName("EventStatusId")
            .IsRequired();

        builder.Property(x => x.EventSeverityId)
            .HasConversion(id => id.Value, value => new EventSeverityId(value))
            .HasColumnName("EventSeverityId")
            .IsRequired();

        builder.Property(x => x.EventSourceTypeId)
            .HasConversion(id => id.Value, value => new EventSourceTypeId(value))
            .HasColumnName("EventSourceTypeId")
            .IsRequired();

        builder.Property(x => x.EventCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(2000);
        builder.Property(x => x.DetectedAtUtc).IsRequired();
        builder.Property(x => x.ReportedAtUtc).HasColumnName("ReportedAtUtc").IsRequired();
        builder.Property(x => x.ClosedAtUtc);
        builder.Property(x => x.ReportedByUserId).HasColumnName("ReportedByUserId");
        builder.Property(x => x.OwnerUserId).HasColumnName("OwnerUserId");
        builder.Property(x => x.OwningPersonId).HasColumnName("OwningPersonId");
        builder.Property(x => x.SourceReference).HasMaxLength(200);
        builder.Property(x => x.IsDrill).HasColumnName("IsDrill").IsRequired();

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.EventCode).IsUnique().HasDatabaseName("UQ_EventManagement_OperationalEvent_EventCode");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_EventManagement_OperationalEvent_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EventType>()
            .WithMany()
            .HasForeignKey(x => x.EventTypeId)
            .HasConstraintName("FK_EventManagement_OperationalEvent_EventType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EventStatus>()
            .WithMany()
            .HasForeignKey(x => x.EventStatusId)
            .HasConstraintName("FK_EventManagement_OperationalEvent_EventStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EventSeverity>()
            .WithMany()
            .HasForeignKey(x => x.EventSeverityId)
            .HasConstraintName("FK_EventManagement_OperationalEvent_EventSeverity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EventSourceType>()
            .WithMany()
            .HasForeignKey(x => x.EventSourceTypeId)
            .HasConstraintName("FK_EventManagement_OperationalEvent_EventSourceType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
