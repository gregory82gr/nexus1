using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSetting", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_AppSetting");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AppSettingId(value))
            .HasColumnName("AppSettingId")
            .ValueGeneratedNever();

        builder.Property(x => x.Key).HasColumnName("Key").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Value).HasColumnName("Value").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ValueType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsEncrypted).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsSystem).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique().HasDatabaseName("UQ_CorePlatform_AppSetting_Key");

        builder.Ignore(x => x.DomainEvents);
    }
}
