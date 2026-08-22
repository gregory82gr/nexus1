using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class PersonnelRequirementConfiguration : IEntityTypeConfiguration<PersonnelRequirement>
{
    public void Configure(EntityTypeBuilder<PersonnelRequirement> builder)
    {
        builder.ToTable("PersonnelRequirement", "Organization", t =>
        {
            t.HasCheckConstraint("CK_Organization_PersonnelRequirement_MinRequiredCount", "[MinRequiredCount] >= 0");
            t.HasCheckConstraint("CK_Organization_PersonnelRequirement_Validity", "[ValidToUtc] IS NULL OR [ValidToUtc] > [ValidFromUtc]");
        });
        builder.HasKey(x => x.Id).HasName("PK_Organization_PersonnelRequirement");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PersonnelRequirementId(value))
            .HasColumnName("PersonnelRequirementId")
            .ValueGeneratedNever();

        builder.Property(x => x.SiteId)
            .HasConversion(id => id.Value, value => new SiteId(value))
            .HasColumnName("SiteId")
            .IsRequired();

        builder.Property(x => x.PlantId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new PlantId(value.Value) : (PlantId?)null)
            .HasColumnName("PlantId");

        builder.Property(x => x.DepartmentId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new DepartmentId(value.Value) : (DepartmentId?)null)
            .HasColumnName("DepartmentId");

        builder.Property(x => x.PositionId)
            .HasConversion(id => id.Value, value => new PositionId(value))
            .HasColumnName("PositionId")
            .IsRequired();

        builder.Property(x => x.MinRequiredCount).IsRequired();

        builder.Property(x => x.RequiredQualificationId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new QualificationId(value.Value) : (QualificationId?)null)
            .HasColumnName("RequiredQualificationId");

        builder.Property(x => x.IsSafetyCritical).IsRequired();
        builder.Property(x => x.ValidFromUtc).IsRequired();
        builder.Property(x => x.ValidToUtc);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<Site>().WithMany().HasForeignKey(x => x.SiteId)
            .HasConstraintName("FK_Organization_PersonnelRequirement_Site")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Plant>().WithMany().HasForeignKey(x => x.PlantId)
            .HasConstraintName("FK_Organization_PersonnelRequirement_Plant")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("FK_Organization_PersonnelRequirement_Department")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>().WithMany().HasForeignKey(x => x.PositionId)
            .HasConstraintName("FK_Organization_PersonnelRequirement_Position")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Qualification>().WithMany().HasForeignKey(x => x.RequiredQualificationId)
            .HasConstraintName("FK_Organization_PersonnelRequirement_RequiredQualification")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
