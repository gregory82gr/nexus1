using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

/// <summary>
/// High-volume fact table — the atlas DDL deliberately declares
/// PRIMARY KEY NONCLUSTERED (C.5.4.7), not the default clustered PK every
/// other table in this sector uses. Mapped via KeyBuilder.IsClustered(false)
/// (Microsoft.EntityFrameworkCore.SqlServer extension) — this maps cleanly,
/// no workaround needed.
/// </summary>
public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.ToTable("Measurement", "Instrumentation", tb => tb.HasCheckConstraint(
            "CK_Instrumentation_Measurement_OneValue", "[NumericValue] IS NOT NULL OR [TextValue] IS NOT NULL"));

        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_Measurement");
        builder.HasKey(x => x.Id).IsClustered(false);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MeasurementId(value))
            .HasColumnName("MeasurementId")
            .ValueGeneratedNever();

        builder.Property(x => x.SignalId)
            .HasConversion(id => id.Value, value => new SignalId(value))
            .HasColumnName("SignalId")
            .IsRequired();

        builder.Property(x => x.SignalQualityId)
            .HasConversion(id => id.Value, value => new SignalQualityId(value))
            .HasColumnName("SignalQualityId")
            .IsRequired();

        builder.Property(x => x.MeasurementSourceId)
            .HasConversion(id => id.Value, value => new MeasurementSourceId(value))
            .HasColumnName("MeasurementSourceId")
            .IsRequired();

        builder.Property(x => x.TimestampUtc).IsRequired();
        builder.Property(x => x.NumericValue);
        builder.Property(x => x.TextValue).HasMaxLength(400);
        builder.Property(x => x.IsEstimated).IsRequired();
        builder.Property(x => x.InsertedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.SignalId, x.TimestampUtc }).HasDatabaseName("IX_Instrumentation_Measurement_SignalId_TimestampUtc");

        builder.HasOne<Signal>()
            .WithMany()
            .HasForeignKey(x => x.SignalId)
            .HasConstraintName("FK_Instrumentation_Measurement_Signal")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SignalQuality>()
            .WithMany()
            .HasForeignKey(x => x.SignalQualityId)
            .HasConstraintName("FK_Instrumentation_Measurement_SignalQuality")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MeasurementSource>()
            .WithMany()
            .HasForeignKey(x => x.MeasurementSourceId)
            .HasConstraintName("FK_Instrumentation_Measurement_MeasurementSource")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
