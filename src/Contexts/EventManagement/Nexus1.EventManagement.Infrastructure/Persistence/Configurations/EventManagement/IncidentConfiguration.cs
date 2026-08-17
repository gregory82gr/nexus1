using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

/// <summary>
/// Same audit shadow-property treatment as OperationalEventConfiguration.
/// LeadInvestigatorUserId carries a real FK to Security.ApplicationUser in
/// the atlas's own single-database DDL, but SecurityDb is a different
/// physical database, so it stays passport-only.
///
/// INVARIANT (ADR-022, see Incident.cs's own doc comment): OperationalEventId
/// is UNIQUE — at most one Incident per OperationalEvent, matching the
/// atlas's own design choice ("specialized records anchored to one
/// OperationalEvent"). Enforced here as a real unique index/constraint,
/// backed at the Application layer by OpenIncidentCommandHandler's own
/// pre-check via IIncidentExistenceFinder.
/// </summary>
public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incident", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_Incident");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new IncidentId(value))
            .HasColumnName("IncidentId")
            .ValueGeneratedNever();

        builder.Property(x => x.OperationalEventId)
            .HasConversion(id => id.Value, value => new OperationalEventId(value))
            .HasColumnName("OperationalEventId")
            .IsRequired();

        builder.Property(x => x.IncidentTypeId)
            .HasConversion(id => id.Value, value => new IncidentTypeId(value))
            .HasColumnName("IncidentTypeId")
            .IsRequired();

        builder.Property(x => x.IncidentStatusId)
            .HasConversion(id => id.Value, value => new IncidentStatusId(value))
            .HasColumnName("IncidentStatusId")
            .IsRequired();

        builder.Property(x => x.IncidentNumber).HasMaxLength(80).IsRequired();
        builder.Property(x => x.InvestigationSummary).HasMaxLength(2000);
        builder.Property(x => x.OpenedAtUtc).HasColumnName("OpenedAtUtc").IsRequired();
        builder.Property(x => x.ClosedAtUtc);
        builder.Property(x => x.LeadInvestigatorUserId).HasColumnName("LeadInvestigatorUserId");

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.OperationalEventId).IsUnique().HasDatabaseName("UQ_EventManagement_Incident_OperationalEvent");
        builder.HasIndex(x => x.IncidentNumber).IsUnique().HasDatabaseName("UQ_EventManagement_Incident_IncidentNumber");

        builder.HasOne<OperationalEvent>()
            .WithMany()
            .HasForeignKey(x => x.OperationalEventId)
            .HasConstraintName("FK_EventManagement_Incident_OperationalEvent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncidentType>()
            .WithMany()
            .HasForeignKey(x => x.IncidentTypeId)
            .HasConstraintName("FK_EventManagement_Incident_IncidentType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncidentStatus>()
            .WithMany()
            .HasForeignKey(x => x.IncidentStatusId)
            .HasConstraintName("FK_EventManagement_Incident_IncidentStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
