using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

/// <summary>
/// Same audit shadow-property treatment as OperationalEventConfiguration/
/// IncidentConfiguration. VerifiedByUserId carries a real FK to
/// Security.ApplicationUser in the atlas's own single-database DDL, but
/// SecurityDb is a different physical database, so it stays passport-only.
///
/// CompletedAtUtc/VerifiedAtUtc/VerifiedByUserId have private setters on the
/// domain type (IncidentAction.Complete/.Verify) — EF Core maps these via
/// its normal non-public-setter support, same as every other property here.
/// </summary>
public sealed class IncidentActionConfiguration : IEntityTypeConfiguration<IncidentAction>
{
    public void Configure(EntityTypeBuilder<IncidentAction> builder)
    {
        builder.ToTable("IncidentAction", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_IncidentAction");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new IncidentActionId(value))
            .HasColumnName("IncidentActionId")
            .ValueGeneratedNever();

        builder.Property(x => x.IncidentId)
            .HasConversion(id => id.Value, value => new IncidentId(value))
            .HasColumnName("IncidentId")
            .IsRequired();

        builder.Property(x => x.IncidentActionTypeId)
            .HasConversion(id => id.Value, value => new IncidentActionTypeId(value))
            .HasColumnName("IncidentActionTypeId")
            .IsRequired();

        builder.Property(x => x.IncidentActionStatusId)
            .HasConversion(id => id.Value, value => new IncidentActionStatusId(value))
            .HasColumnName("IncidentActionStatusId")
            .IsRequired();

        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.DueAtUtc);
        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.VerifiedAtUtc);
        builder.Property(x => x.VerifiedByUserId).HasColumnName("VerifiedByUserId");

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne<Incident>()
            .WithMany()
            .HasForeignKey(x => x.IncidentId)
            .HasConstraintName("FK_EventManagement_IncidentAction_Incident")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncidentActionType>()
            .WithMany()
            .HasForeignKey(x => x.IncidentActionTypeId)
            .HasConstraintName("FK_EventManagement_IncidentAction_Type")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncidentActionStatus>()
            .WithMany()
            .HasForeignKey(x => x.IncidentActionStatusId)
            .HasConstraintName("FK_EventManagement_IncidentAction_Status")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
