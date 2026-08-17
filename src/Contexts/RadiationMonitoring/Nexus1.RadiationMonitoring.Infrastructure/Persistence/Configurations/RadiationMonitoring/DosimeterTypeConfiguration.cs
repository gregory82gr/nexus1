using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Configurations.RadiationMonitoring;

/// <summary>ModifiedAtUtc/RowVersion are EF-only shadow properties, not Domain-modeled (ADR-024).</summary>
public sealed class DosimeterTypeConfiguration : IEntityTypeConfiguration<DosimeterType>
{
    public void Configure(EntityTypeBuilder<DosimeterType> builder)
    {
        builder.ToTable("DosimeterType", "RadiationMonitoring");
        builder.HasKey(x => x.Id).HasName("PK_RadiationMonitoring_DosimeterType");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DosimeterTypeId(value))
            .HasColumnName("DosimeterTypeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_RadiationMonitoring_DosimeterType_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
