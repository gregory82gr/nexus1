using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Team", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_Team");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TeamId(value))
            .HasColumnName("TeamId")
            .ValueGeneratedNever();

        builder.Property(x => x.DepartmentId)
            .HasConversion(id => id.Value, value => new DepartmentId(value))
            .HasColumnName("DepartmentId")
            .IsRequired();

        builder.Property(x => x.TeamTypeId)
            .HasConversion(id => id.Value, value => new TeamTypeId(value))
            .HasColumnName("TeamTypeId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsShiftTeam).IsRequired();
        builder.Property(x => x.IsEmergencyTeam).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.DepartmentId, x.Code }).IsUnique().HasDatabaseName("UQ_Organization_Team_Department_Code");

        builder.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("FK_Organization_Team_Department")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TeamType>().WithMany().HasForeignKey(x => x.TeamTypeId)
            .HasConstraintName("FK_Organization_Team_TeamType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
