using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// Full audit shape mapped as EF shadow properties only. AssemblyPointId
/// and RouteStatusId are real internal FKs, NOT NULL. SiteId/PlantId are
/// passport-only — Organization.Site/Plant live in OrganizationDb
/// (ADR-025).
/// </summary>
public sealed class EvacuationRouteConfiguration : IEntityTypeConfiguration<EvacuationRoute>
{
    public void Configure(EntityTypeBuilder<EvacuationRoute> builder)
    {
        builder.ToTable("EvacuationRoute", "EmergencyPreparedness", t => t.HasCheckConstraint(
            "CK_EmergencyPreparedness_EvacuationRoute_EstimatedMinutes", "[EstimatedMinutes] IS NULL OR [EstimatedMinutes] >= 0"));
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_EvacuationRoute");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EvacuationRouteId(value))
            .HasColumnName("EvacuationRouteId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SiteId).HasColumnName("SiteId").IsRequired();
        builder.Property(x => x.PlantId).HasColumnName("PlantId");

        builder.Property(x => x.AssemblyPointId)
            .HasConversion(id => id.Value, value => new AssemblyPointId(value))
            .HasColumnName("AssemblyPointId")
            .IsRequired();

        builder.Property(x => x.RouteStatusId)
            .HasConversion(id => id.Value, value => new RouteStatusId(value))
            .HasColumnName("RouteStatusId")
            .IsRequired();

        builder.Property(x => x.FromLocation).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EstimatedMinutes);
        builder.Property(x => x.RouteGeometryJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EmergencyPreparedness_EvacuationRoute_Code");

        builder.HasOne<AssemblyPoint>()
            .WithMany()
            .HasForeignKey(x => x.AssemblyPointId)
            .HasConstraintName("FK_EmergencyPreparedness_EvacuationRoute_AssemblyPoint")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RouteStatus>()
            .WithMany()
            .HasForeignKey(x => x.RouteStatusId)
            .HasConstraintName("FK_EmergencyPreparedness_EvacuationRoute_RouteStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
