using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

/// <summary>No ModifiedAtUtc/ModifiedBy/IsDeleted — the atlas DDL genuinely gives this table none (leaner than most, verified against the atlas, ADR-019).</summary>
public sealed class SignalMappingConfiguration : IEntityTypeConfiguration<SignalMapping>
{
    public void Configure(EntityTypeBuilder<SignalMapping> builder)
    {
        builder.ToTable("SignalMapping", "Instrumentation", tb => tb.HasCheckConstraint(
            "CK_Instrumentation_SignalMapping_Effective", "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]"));
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_SignalMapping");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SignalMappingId(value))
            .HasColumnName("SignalMappingId")
            .ValueGeneratedNever();

        builder.Property(x => x.SignalId)
            .HasConversion(id => id.Value, value => new SignalId(value))
            .HasColumnName("SignalId")
            .IsRequired();

        builder.Property(x => x.AcquisitionPointId)
            .HasConversion(id => id.Value, value => new AcquisitionPointId(value))
            .HasColumnName("AcquisitionPointId")
            .IsRequired();

        builder.Property(x => x.EffectiveFromUtc).IsRequired();
        builder.Property(x => x.EffectiveToUtc);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.SignalId, x.AcquisitionPointId, x.EffectiveFromUtc })
            .IsUnique()
            .HasDatabaseName("UQ_Instrumentation_SignalMapping_Signal_Point_From");

        builder.HasOne<Signal>()
            .WithMany()
            .HasForeignKey(x => x.SignalId)
            .HasConstraintName("FK_Instrumentation_SignalMapping_Signal")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AcquisitionPoint>()
            .WithMany()
            .HasForeignKey(x => x.AcquisitionPointId)
            .HasConstraintName("FK_Instrumentation_SignalMapping_AcquisitionPoint")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
