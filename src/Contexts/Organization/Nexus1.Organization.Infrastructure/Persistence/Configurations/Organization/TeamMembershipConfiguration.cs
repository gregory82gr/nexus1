using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class TeamMembershipConfiguration : IEntityTypeConfiguration<TeamMembership>
{
    public void Configure(EntityTypeBuilder<TeamMembership> builder)
    {
        builder.ToTable("TeamMembership", "Organization", t => t.HasCheckConstraint(
            "CK_Organization_TeamMembership_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]"));
        builder.HasKey(x => x.Id).HasName("PK_Organization_TeamMembership");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TeamMembershipId(value))
            .HasColumnName("TeamMembershipId")
            .ValueGeneratedNever();

        builder.Property(x => x.PersonId)
            .HasConversion(id => id.Value, value => new PersonId(value))
            .HasColumnName("PersonId")
            .IsRequired();

        builder.Property(x => x.TeamId)
            .HasConversion(id => id.Value, value => new TeamId(value))
            .HasColumnName("TeamId")
            .IsRequired();

        builder.Property(x => x.PositionId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new PositionId(value.Value) : (PositionId?)null)
            .HasColumnName("PositionId");

        builder.Property(x => x.StartDate).HasColumnName("StartDate").HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnName("EndDate").HasColumnType("date");
        builder.Property(x => x.IsLead).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<Person>().WithMany().HasForeignKey(x => x.PersonId)
            .HasConstraintName("FK_Organization_TeamMembership_Person")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId)
            .HasConstraintName("FK_Organization_TeamMembership_Team")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>().WithMany().HasForeignKey(x => x.PositionId)
            .HasConstraintName("FK_Organization_TeamMembership_Position")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
