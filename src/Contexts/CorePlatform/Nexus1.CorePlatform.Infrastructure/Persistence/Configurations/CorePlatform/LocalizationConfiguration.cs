using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class LocalizationConfiguration : IEntityTypeConfiguration<Localization>
{
    public void Configure(EntityTypeBuilder<Localization> builder)
    {
        builder.ToTable("Localization", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_Localization");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new LocalizationId(value))
            .HasColumnName("LocalizationId")
            .ValueGeneratedNever();

        builder.Property(x => x.ResourceKey).HasMaxLength(300).IsRequired();

        builder.Property(x => x.LanguageId)
            .HasConversion(id => id.Value, value => new LanguageId(value))
            .HasColumnName("LanguageId")
            .IsRequired();

        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.SourceText);
        builder.Property(x => x.IsMachineTranslated).IsRequired();
        builder.Property(x => x.LastReviewedAtUtc);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        // Atlas declares a plain FK with no ON DELETE clause (SQL Server default:
        // NO ACTION) — EF Core's own default for a required FK is CASCADE, which
        // would silently delete every Localization row when its Language is
        // deleted. Restrict matches the atlas's actual behavior.
        builder.HasOne<Language>().WithMany().HasForeignKey(x => x.LanguageId)
            .HasConstraintName("FK_CorePlatform_Localization_Language")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ResourceKey, x.LanguageId })
            .IsUnique()
            .HasDatabaseName("UQ_CorePlatform_Localization_ResourceKey_Language");

        builder.HasIndex(x => x.LanguageId).HasDatabaseName("IX_CorePlatform_Localization_LanguageId");
        builder.HasIndex(x => x.ResourceKey).HasDatabaseName("IX_CorePlatform_Localization_ResourceKey");

        builder.Ignore(x => x.DomainEvents);
    }
}
