using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// No audit columns beyond RowVersion, mapped as an EF-only shadow
/// property — matches the real DDL exactly (append-only fact table).
/// EmergencyResourceId and ReadinessStatusId are real internal FKs, NOT
/// NULL. CheckedByUserId is passport-only — Security.ApplicationUser lives
/// in SecurityDb (ADR-025). Id is bigint (ResourceReadinessCheckId(long)).
/// </summary>
public sealed class ResourceReadinessCheckConfiguration : IEntityTypeConfiguration<ResourceReadinessCheck>
{
    public void Configure(EntityTypeBuilder<ResourceReadinessCheck> builder)
    {
        builder.ToTable("ResourceReadinessCheck", "EmergencyPreparedness");
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_ResourceReadinessCheck");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ResourceReadinessCheckId(value))
            .HasColumnName("ResourceReadinessCheckId")
            .ValueGeneratedNever();

        builder.Property(x => x.EmergencyResourceId)
            .HasConversion(id => id.Value, value => new EmergencyResourceId(value))
            .HasColumnName("EmergencyResourceId")
            .IsRequired();

        builder.Property(x => x.ReadinessStatusId)
            .HasConversion(id => id.Value, value => new ReadinessStatusId(value))
            .HasColumnName("ReadinessStatusId")
            .IsRequired();

        builder.Property(x => x.CheckedAtUtc).IsRequired();
        builder.Property(x => x.CheckedByUserId).HasColumnName("CheckedByUserId").IsRequired();
        builder.Property(x => x.ConditionSummary).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.NextCheckDueUtc);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne<EmergencyResource>()
            .WithMany()
            .HasForeignKey(x => x.EmergencyResourceId)
            .HasConstraintName("FK_EmergencyPreparedness_ResourceReadinessCheck_EmergencyResource")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReadinessStatus>()
            .WithMany()
            .HasForeignKey(x => x.ReadinessStatusId)
            .HasConstraintName("FK_EmergencyPreparedness_ResourceReadinessCheck_ReadinessStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
