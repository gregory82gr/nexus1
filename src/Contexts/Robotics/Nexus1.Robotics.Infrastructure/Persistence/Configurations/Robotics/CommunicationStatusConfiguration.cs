using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>ModifiedAtUtc/RowVersion are EF-only shadow properties, not Domain-modeled (ADR-023).</summary>
public sealed class CommunicationStatusConfiguration : IEntityTypeConfiguration<CommunicationStatus>
{
    public void Configure(EntityTypeBuilder<CommunicationStatus> builder)
    {
        builder.ToTable("CommunicationStatus", "Robotics");
        builder.HasKey(x => x.Id).HasName("PK_Robotics_CommunicationStatus");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new CommunicationStatusId(value))
            .HasColumnName("CommunicationStatusId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Robotics_CommunicationStatus_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
