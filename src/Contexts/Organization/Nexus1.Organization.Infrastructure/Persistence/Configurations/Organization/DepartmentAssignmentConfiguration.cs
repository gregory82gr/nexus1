using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class DepartmentAssignmentConfiguration : IEntityTypeConfiguration<DepartmentAssignment>
{
    public void Configure(EntityTypeBuilder<DepartmentAssignment> builder)
    {
        builder.ToTable("DepartmentAssignment", "Organization", t => t.HasCheckConstraint(
            "CK_Organization_DepartmentAssignment_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]"));
        builder.HasKey(x => x.Id).HasName("PK_Organization_DepartmentAssignment");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DepartmentAssignmentId(value))
            .HasColumnName("DepartmentAssignmentId")
            .ValueGeneratedNever();

        builder.Property(x => x.PersonId)
            .HasConversion(id => id.Value, value => new PersonId(value))
            .HasColumnName("PersonId")
            .IsRequired();

        builder.Property(x => x.DepartmentId)
            .HasConversion(id => id.Value, value => new DepartmentId(value))
            .HasColumnName("DepartmentId")
            .IsRequired();

        builder.Property(x => x.PositionId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new PositionId(value.Value) : (PositionId?)null)
            .HasColumnName("PositionId");

        builder.Property(x => x.StartDate).HasColumnName("StartDate").HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnName("EndDate").HasColumnType("date");
        builder.Property(x => x.IsPrimary).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<Person>().WithMany().HasForeignKey(x => x.PersonId)
            .HasConstraintName("FK_Organization_DepartmentAssignment_Person")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("FK_Organization_DepartmentAssignment_Department")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>().WithMany().HasForeignKey(x => x.PositionId)
            .HasConstraintName("FK_Organization_DepartmentAssignment_Position")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
