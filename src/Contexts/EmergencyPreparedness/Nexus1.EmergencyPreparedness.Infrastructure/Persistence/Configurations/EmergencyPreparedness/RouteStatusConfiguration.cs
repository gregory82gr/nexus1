using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>ModifiedAtUtc/RowVersion are EF-only shadow properties, not Domain-modeled (ADR-025).</summary>
public sealed class RouteStatusConfiguration : IEntityTypeConfiguration<RouteStatus>
{
    public void Configure(EntityTypeBuilder<RouteStatus> builder)
    {
        builder.ToTable("RouteStatus", "EmergencyPreparedness");
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_RouteStatus");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RouteStatusId(value))
            .HasColumnName("RouteStatusId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EmergencyPreparedness_RouteStatus_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
