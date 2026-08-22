using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence.Configurations.Robotics;

/// <summary>
/// No audit columns — lean, append-only chronology, mirrors
/// EventManagement.EventTimelineEntryConfiguration's own shape (ADR-023).
/// RecordedByUserId stays passport-only — Security.ApplicationUser, a
/// different physical database.
/// </summary>
public sealed class MissionEventConfiguration : IEntityTypeConfiguration<MissionEvent>
{
    public void Configure(EntityTypeBuilder<MissionEvent> builder)
    {
        builder.ToTable("MissionEvent", "Robotics");
        builder.HasKey(x => x.Id).HasName("PK_Robotics_MissionEvent");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MissionEventId(value))
            .HasColumnName("MissionEventId")
            .ValueGeneratedNever();

        builder.Property(x => x.MissionId)
            .HasConversion(id => id.Value, value => new MissionId(value))
            .HasColumnName("MissionId")
            .IsRequired();

        builder.Property(x => x.RobotId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (int?)null, value => value.HasValue ? new RobotId(value.Value) : (RobotId?)null)
            .HasColumnName("RobotId");

        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.EventCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Detail).HasMaxLength(2000);
        builder.Property(x => x.IsFault).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.RecordedByUserId).HasColumnName("RecordedByUserId");

        builder.HasOne<Mission>()
            .WithMany()
            .HasForeignKey(x => x.MissionId)
            .HasConstraintName("FK_Robotics_MissionEvent_Mission")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Robot>()
            .WithMany()
            .HasForeignKey(x => x.RobotId)
            .HasConstraintName("FK_Robotics_MissionEvent_Robot")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
