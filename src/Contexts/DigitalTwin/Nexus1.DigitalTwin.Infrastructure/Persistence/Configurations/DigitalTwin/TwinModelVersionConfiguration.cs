using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>Same shadow-property audit treatment as TwinModelConfiguration — see its XML doc for the full rationale.</summary>
public sealed class TwinModelVersionConfiguration : IEntityTypeConfiguration<TwinModelVersion>
{
    public void Configure(EntityTypeBuilder<TwinModelVersion> builder)
    {
        builder.ToTable("TwinModelVersion", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinModelVersion");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinModelVersionId(value))
            .HasColumnName("TwinModelVersionId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinModelId)
            .HasConversion(id => id.Value, value => new TwinModelId(value))
            .HasColumnName("TwinModelId")
            .IsRequired();

        builder.Property(x => x.SolverTypeId)
            .HasConversion(id => id.Value, value => new SolverTypeId(value))
            .HasColumnName("SolverTypeId")
            .IsRequired();

        builder.Property(x => x.ValidationStatusId)
            .HasConversion(id => id.Value, value => new ValidationStatusId(value))
            .HasColumnName("ValidationStatusId")
            .IsRequired();

        builder.Property(x => x.VersionCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ModelVersion).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SourceReference).HasMaxLength(500);
        builder.Property(x => x.ModelHash).HasColumnType("varbinary(32)");
        builder.Property(x => x.ConfigurationJson);
        builder.Property(x => x.ReleasedAtUtc);
        builder.Property(x => x.ApprovedByUserId).HasColumnName("ApprovedByUserId");
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.TwinModelId, x.VersionCode }).IsUnique().HasDatabaseName("UQ_DigitalTwin_TwinModelVersion_Model_Version");

        builder.HasOne<TwinModel>()
            .WithMany()
            .HasForeignKey(x => x.TwinModelId)
            .HasConstraintName("FK_DigitalTwin_TwinModelVersion_TwinModel")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SolverType>()
            .WithMany()
            .HasForeignKey(x => x.SolverTypeId)
            .HasConstraintName("FK_DigitalTwin_TwinModelVersion_SolverType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ValidationStatus>()
            .WithMany()
            .HasForeignKey(x => x.ValidationStatusId)
            .HasConstraintName("FK_DigitalTwin_TwinModelVersion_ValidationStatus")
            .OnDelete(DeleteBehavior.Restrict);

        // ApprovedByUserId -> Security.ApplicationUser: no enforced FK, SecurityDb is a separate physical database (ADR-020).
        builder.Ignore(x => x.DomainEvents);
    }
}
