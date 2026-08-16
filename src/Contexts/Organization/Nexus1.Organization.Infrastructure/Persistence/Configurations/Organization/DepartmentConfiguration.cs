using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Department", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_Department");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DepartmentId(value))
            .HasColumnName("DepartmentId")
            .ValueGeneratedNever();

        builder.Property(x => x.LegalEntityId)
            .HasConversion(id => id.Value, value => new LegalEntityId(value))
            .HasColumnName("LegalEntityId")
            .IsRequired();

        builder.Property(x => x.ParentDepartmentId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new DepartmentId(value.Value) : (DepartmentId?)null)
            .HasColumnName("ParentDepartmentId");

        builder.Property(x => x.DepartmentTypeId)
            .HasConversion(id => id.Value, value => new DepartmentTypeId(value))
            .HasColumnName("DepartmentTypeId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CostCentreCode).HasMaxLength(50);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.LegalEntityId, x.Code }).IsUnique().HasDatabaseName("UQ_Organization_Department_LegalEntity_Code");

        builder.HasOne<LegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId)
            .HasConstraintName("FK_Organization_Department_LegalEntity")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Department>().WithMany().HasForeignKey(x => x.ParentDepartmentId)
            .HasConstraintName("FK_Organization_Department_ParentDepartment")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DepartmentType>().WithMany().HasForeignKey(x => x.DepartmentTypeId)
            .HasConstraintName("FK_Organization_Department_DepartmentType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
