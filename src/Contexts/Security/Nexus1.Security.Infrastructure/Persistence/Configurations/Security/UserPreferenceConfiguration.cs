using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence.Configurations.Security;

/// <summary>
/// LanguageId/TimeZoneId are plain passport columns with no FK constraint —
/// CorePlatform lives in a different physical database (AlarmManagementDb
/// vs SecurityDb), so a real cross-database FOREIGN KEY is not possible
/// (ADR-016's correction to ADR-015's cross-schema-FK exception).
/// </summary>
public sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreference", "Security");
        builder.HasKey(x => x.Id).HasName("PK_Security_UserPreference");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ApplicationUserId(value))
            .HasColumnName("ApplicationUserId")
            .ValueGeneratedNever();

        builder.Property(x => x.LanguageId).HasColumnName("LanguageId").IsRequired();
        builder.Property(x => x.TimeZoneId).HasColumnName("TimeZoneId").IsRequired();
        builder.Property(x => x.Theme).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DateFormat).HasMaxLength(30);
        builder.Property(x => x.NumberFormat).HasMaxLength(30);
        builder.Property(x => x.ReceiveEmailAlerts).IsRequired();
        builder.Property(x => x.ReceiveInAppAlerts).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<ApplicationUser>().WithOne().HasForeignKey<UserPreference>(x => x.Id)
            .HasConstraintName("FK_Security_UserPreference_ApplicationUser")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
