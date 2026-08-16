using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("FeatureFlag", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_FeatureFlag");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new FeatureFlagId(value))
            .HasColumnName("FeatureFlagId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.EnvironmentName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.EnabledFromUtc);
        builder.Property(x => x.ExpiresAtUtc);
        builder.Property(x => x.Owner).HasMaxLength(150);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.Code, x.EnvironmentName })
            .IsUnique()
            .HasDatabaseName("UQ_CorePlatform_FeatureFlag_Code_Environment");

        builder.Ignore(x => x.DomainEvents);
    }
}
