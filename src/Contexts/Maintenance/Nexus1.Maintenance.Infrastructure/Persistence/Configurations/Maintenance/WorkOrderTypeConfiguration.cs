using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

public sealed class WorkOrderTypeConfiguration : IEntityTypeConfiguration<WorkOrderType>
{
    public void Configure(EntityTypeBuilder<WorkOrderType> builder)
    {
        builder.ToTable("WorkOrderType", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_WorkOrderType");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new WorkOrderTypeId(value))
            .HasColumnName("WorkOrderTypeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Maintenance_WorkOrderType_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
