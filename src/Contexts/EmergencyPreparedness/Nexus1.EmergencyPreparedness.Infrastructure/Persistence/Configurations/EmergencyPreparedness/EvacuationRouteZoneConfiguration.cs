using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// No audit columns. EvacuationRouteId is a real internal FK, NOT NULL.
/// RadiationZoneId carries a real FK to RadiationMonitoring.RadiationZone
/// via RadiationMonitoringRadiationZoneReference, named
/// FK_EvacuationRouteZone_RadiationZone verbatim per ADR-025's own
/// evidence-required section, NOT NULL, unique together with
/// EvacuationRouteId.
/// </summary>
public sealed class EvacuationRouteZoneConfiguration : IEntityTypeConfiguration<EvacuationRouteZone>
{
    public void Configure(EntityTypeBuilder<EvacuationRouteZone> builder)
    {
        builder.ToTable("EvacuationRouteZone", "EmergencyPreparedness");
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_EvacuationRouteZone");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EvacuationRouteZoneId(value))
            .HasColumnName("EvacuationRouteZoneId")
            .ValueGeneratedNever();

        builder.Property(x => x.EvacuationRouteId)
            .HasConversion(id => id.Value, value => new EvacuationRouteId(value))
            .HasColumnName("EvacuationRouteId")
            .IsRequired();

        builder.Property(x => x.RadiationZoneId).HasColumnName("RadiationZoneId").IsRequired();
        builder.Property(x => x.IsAvoidIfAlarmed).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.EvacuationRouteId, x.RadiationZoneId }).IsUnique()
            .HasDatabaseName("UQ_EmergencyPreparedness_EvacuationRouteZone_Route_Zone");

        builder.HasOne<EvacuationRoute>()
            .WithMany()
            .HasForeignKey(x => x.EvacuationRouteId)
            .HasConstraintName("FK_EmergencyPreparedness_EvacuationRouteZone_EvacuationRoute")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RadiationMonitoringRadiationZoneReference>()
            .WithMany()
            .HasForeignKey(x => x.RadiationZoneId)
            .HasConstraintName("FK_EvacuationRouteZone_RadiationZone")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
