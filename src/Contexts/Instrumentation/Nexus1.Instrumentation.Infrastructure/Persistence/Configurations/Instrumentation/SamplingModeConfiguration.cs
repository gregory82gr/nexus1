using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

public sealed class SamplingModeConfiguration : IEntityTypeConfiguration<SamplingMode>
{
    public void Configure(EntityTypeBuilder<SamplingMode> builder)
    {
        builder.ToTable("SamplingMode", "Instrumentation");
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_SamplingMode");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SamplingModeId(value))
            .HasColumnName("SamplingModeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Instrumentation_SamplingMode_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
