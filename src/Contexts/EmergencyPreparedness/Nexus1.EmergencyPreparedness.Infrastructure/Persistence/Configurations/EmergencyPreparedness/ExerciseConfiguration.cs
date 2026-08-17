using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Configurations.EmergencyPreparedness;

/// <summary>
/// Full audit shape mapped as EF shadow properties only. ExerciseTypeId/
/// ExerciseStatusId are real internal FKs, NOT NULL. SiteId/PlantId/
/// CoordinatorUserId are passport-only plain columns — Organization.Site/
/// Plant live in OrganizationDb, Security.ApplicationUser lives in
/// SecurityDb (ADR-025). EmergencyScenarioId is deliberately not mapped at
/// all — out of scope this pass.
/// </summary>
public sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercise", "EmergencyPreparedness", t =>
        {
            t.HasCheckConstraint("CK_EmergencyPreparedness_Exercise_ScheduledDateRange", "[ScheduledEndUtc] >= [ScheduledStartUtc]");
            t.HasCheckConstraint("CK_EmergencyPreparedness_Exercise_ActualDateRange", "[ActualEndUtc] IS NULL OR [ActualStartUtc] IS NULL OR [ActualEndUtc] >= [ActualStartUtc]");
        });
        builder.HasKey(x => x.Id).HasName("PK_EmergencyPreparedness_Exercise");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ExerciseId(value))
            .HasColumnName("ExerciseId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.Property(x => x.ExerciseTypeId)
            .HasConversion(id => id.Value, value => new ExerciseTypeId(value))
            .HasColumnName("ExerciseTypeId")
            .IsRequired();

        builder.Property(x => x.ExerciseStatusId)
            .HasConversion(id => id.Value, value => new ExerciseStatusId(value))
            .HasColumnName("ExerciseStatusId")
            .IsRequired();

        builder.Property(x => x.SiteId).HasColumnName("SiteId").IsRequired();
        builder.Property(x => x.PlantId).HasColumnName("PlantId");
        builder.Property(x => x.ScheduledStartUtc).IsRequired();
        builder.Property(x => x.ScheduledEndUtc).IsRequired();
        builder.Property(x => x.ActualStartUtc);
        builder.Property(x => x.ActualEndUtc);
        builder.Property(x => x.CoordinatorUserId).HasColumnName("CoordinatorUserId").IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(2000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EmergencyPreparedness_Exercise_Code");

        builder.HasOne<ExerciseType>()
            .WithMany()
            .HasForeignKey(x => x.ExerciseTypeId)
            .HasConstraintName("FK_EmergencyPreparedness_Exercise_ExerciseType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExerciseStatus>()
            .WithMany()
            .HasForeignKey(x => x.ExerciseStatusId)
            .HasConstraintName("FK_EmergencyPreparedness_Exercise_ExerciseStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
