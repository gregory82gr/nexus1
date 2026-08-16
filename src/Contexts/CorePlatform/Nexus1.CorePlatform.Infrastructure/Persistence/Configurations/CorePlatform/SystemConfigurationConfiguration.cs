using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfiguration", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_SystemConfiguration");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SystemConfigurationId(value))
            .HasColumnName("SystemConfigurationId")
            .ValueGeneratedNever();

        builder.Property(x => x.ModuleName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConfigurationKey).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ConfigurationJson).IsRequired();
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.EffectiveFromUtc).IsRequired();
        builder.Property(x => x.EffectiveToUtc);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.ModuleName, x.ConfigurationKey, x.SchemaVersion })
            .IsUnique()
            .HasDatabaseName("UQ_CorePlatform_SystemConfiguration_Module_Key_Version");

        builder.Ignore(x => x.DomainEvents);
    }
}
