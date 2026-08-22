using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

/// <summary>Maps the C# TimeZoneReference type to the atlas's own CorePlatform.TimeZone table name (see TimeZoneReference's own doc comment for why the C# name differs).</summary>
public sealed class TimeZoneReferenceConfiguration : IEntityTypeConfiguration<TimeZoneReference>
{
    public void Configure(EntityTypeBuilder<TimeZoneReference> builder)
    {
        builder.ToTable("TimeZone", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_TimeZone");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TimeZoneReferenceId(value))
            .HasColumnName("TimeZoneId")
            .ValueGeneratedNever();

        builder.Property(x => x.IanaName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.WindowsName).HasMaxLength(100);
        builder.Property(x => x.CurrentUtcOffsetMinutes).IsRequired();
        builder.Property(x => x.ObservesDst).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.IanaName).IsUnique().HasDatabaseName("UQ_CorePlatform_TimeZone_IanaName");

        builder.Ignore(x => x.DomainEvents);
    }
}
