using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Position", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_Position");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PositionId(value))
            .HasColumnName("PositionId")
            .ValueGeneratedNever();

        builder.Property(x => x.DepartmentId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new DepartmentId(value.Value) : (DepartmentId?)null)
            .HasColumnName("DepartmentId");

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsSafetyCritical).IsRequired();
        builder.Property(x => x.RequiresShiftWork).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Organization_Position_Code");

        builder.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("FK_Organization_Position_Department")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
