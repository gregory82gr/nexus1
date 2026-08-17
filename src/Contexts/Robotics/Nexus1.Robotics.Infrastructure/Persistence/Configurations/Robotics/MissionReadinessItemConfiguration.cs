using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>
/// No audit columns (ADR-023). MissionChecklistItemId is deliberately not
/// mapped — the readiness authoring group is out of scope for this pass and
/// does not exist in this codebase.
/// </summary>
public sealed class MissionReadinessItemConfiguration : IEntityTypeConfiguration<MissionReadinessItem>
{
    public void Configure(EntityTypeBuilder<MissionReadinessItem> builder)
    {
        builder.ToTable("MissionReadinessItem", "Robotics");
        builder.HasKey(x => x.Id).HasName("PK_Robotics_MissionReadinessItem");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MissionReadinessItemId(value))
            .HasColumnName("MissionReadinessItemId")
            .ValueGeneratedNever();

        builder.Property(x => x.MissionReadinessAssessmentId)
            .HasConversion(id => id.Value, value => new MissionReadinessAssessmentId(value))
            .HasColumnName("MissionReadinessAssessmentId")
            .IsRequired();

        builder.Property(x => x.ReadinessStatusId)
            .HasConversion(id => id.Value, value => new ReadinessStatusId(value))
            .HasColumnName("ReadinessStatusId")
            .IsRequired();

        builder.Property(x => x.CheckName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Detail).HasMaxLength(1000);
        builder.Property(x => x.IsBlocking).IsRequired().HasDefaultValue(true);

        builder.HasOne<MissionReadinessAssessment>()
            .WithMany()
            .HasForeignKey(x => x.MissionReadinessAssessmentId)
            .HasConstraintName("FK_Robotics_MissionReadinessItem_MissionReadinessAssessment")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReadinessStatus>()
            .WithMany()
            .HasForeignKey(x => x.ReadinessStatusId)
            .HasConstraintName("FK_Robotics_MissionReadinessItem_ReadinessStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
