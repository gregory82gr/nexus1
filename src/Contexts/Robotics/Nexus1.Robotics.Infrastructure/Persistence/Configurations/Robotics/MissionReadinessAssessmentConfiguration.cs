using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>
/// No audit columns (ADR-023). AssessedByUserId stays passport-only —
/// Security.ApplicationUser, a different physical database.
/// </summary>
public sealed class MissionReadinessAssessmentConfiguration : IEntityTypeConfiguration<MissionReadinessAssessment>
{
    public void Configure(EntityTypeBuilder<MissionReadinessAssessment> builder)
    {
        builder.ToTable("MissionReadinessAssessment", "Robotics");
        builder.HasKey(x => x.Id).HasName("PK_Robotics_MissionReadinessAssessment");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MissionReadinessAssessmentId(value))
            .HasColumnName("MissionReadinessAssessmentId")
            .ValueGeneratedNever();

        builder.Property(x => x.MissionId)
            .HasConversion(id => id.Value, value => new MissionId(value))
            .HasColumnName("MissionId")
            .IsRequired();

        builder.Property(x => x.ReadinessStatusId)
            .HasConversion(id => id.Value, value => new ReadinessStatusId(value))
            .HasColumnName("ReadinessStatusId")
            .IsRequired();

        builder.Property(x => x.AssessedAtUtc).IsRequired();
        builder.Property(x => x.AssessedByUserId).HasColumnName("AssessedByUserId");
        builder.Property(x => x.Summary).HasMaxLength(1000);

        builder.HasOne<Mission>()
            .WithMany()
            .HasForeignKey(x => x.MissionId)
            .HasConstraintName("FK_Robotics_MissionReadinessAssessment_Mission")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReadinessStatus>()
            .WithMany()
            .HasForeignKey(x => x.ReadinessStatusId)
            .HasConstraintName("FK_Robotics_MissionReadinessAssessment_ReadinessStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
