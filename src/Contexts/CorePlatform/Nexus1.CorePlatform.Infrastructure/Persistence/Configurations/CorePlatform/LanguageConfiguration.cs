using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Language", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_Language");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new LanguageId(value))
            .HasColumnName("LanguageId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NativeName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsRightToLeft).IsRequired();
        builder.Property(x => x.IsDefault).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_CorePlatform_Language_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
