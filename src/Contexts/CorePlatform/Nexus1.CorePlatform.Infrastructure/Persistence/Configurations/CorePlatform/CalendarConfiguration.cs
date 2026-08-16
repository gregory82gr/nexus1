using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class CalendarConfiguration : IEntityTypeConfiguration<Calendar>
{
    public void Configure(EntityTypeBuilder<Calendar> builder)
    {
        builder.ToTable("Calendar", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_Calendar");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new CalendarId(value))
            .HasColumnName("CalendarId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();

        builder.Property(x => x.TimeZoneId)
            .HasConversion(id => id.Value, value => new TimeZoneReferenceId(value))
            .HasColumnName("TimeZoneId")
            .IsRequired();

        builder.Property(x => x.CalendarType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.WorkingDaysMask).IsRequired();
        builder.Property(x => x.WorkingDayStart).HasColumnType("time(0)");
        builder.Property(x => x.WorkingDayEnd).HasColumnType("time(0)");
        builder.Property(x => x.Is24x7).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        // Atlas declares a plain FK with no ON DELETE clause (SQL Server default:
        // NO ACTION) — EF Core's own default for a required FK is CASCADE.
        // Restrict matches the atlas's actual behavior (see LocalizationConfiguration).
        builder.HasOne<TimeZoneReference>().WithMany().HasForeignKey(x => x.TimeZoneId)
            .HasConstraintName("FK_CorePlatform_Calendar_TimeZone")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_CorePlatform_Calendar_Code");
        builder.HasIndex(x => x.TimeZoneId).HasDatabaseName("IX_CorePlatform_Calendar_TimeZoneId");

        builder.Ignore(x => x.DomainEvents);
    }
}
