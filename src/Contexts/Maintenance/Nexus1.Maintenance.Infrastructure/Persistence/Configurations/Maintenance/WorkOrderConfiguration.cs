using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Maintenance.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>
/// Same shadow-property audit treatment as AssetConfiguration for the full
/// audit-column shape. RequestedAtUtc is the real business timestamp and is
/// domain-modeled; CreatedAtUtc is pure row-insertion bookkeeping and is
/// mapped as a shadow column only. IsDeleted IS domain-modeled — needed by
/// GetOpenWorkOrdersByUnitQuery's own "IsDeleted = 0" filter.
///
/// MaintenancePlanId, MaintenanceWindowId, RequestedByUserId, AssignedTeamId,
/// AssignedPersonId, OriginOperationalEventId and OriginIncidentActionId are
/// all passport-only plain columns with NO HasOne/FK declared at all — see
/// WorkOrder.cs's own XML doc for why each one is out of scope (ADR-021).
/// </summary>
public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrder", "Maintenance");
        builder.HasKey(x => x.Id).HasName("PK_Maintenance_WorkOrder");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new WorkOrderId(value))
            .HasColumnName("WorkOrderId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();

        builder.Property(x => x.AssetId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ? new AssetId(value.Value) : (AssetId?)null)
            .HasColumnName("AssetId");

        builder.Property(x => x.MaintenancePlanId).HasColumnName("MaintenancePlanId");
        builder.Property(x => x.MaintenanceWindowId).HasColumnName("MaintenanceWindowId");

        builder.Property(x => x.WorkOrderTypeId)
            .HasConversion(id => id.Value, value => new WorkOrderTypeId(value))
            .HasColumnName("WorkOrderTypeId")
            .IsRequired();

        builder.Property(x => x.WorkOrderStatusId)
            .HasConversion(id => id.Value, value => new WorkOrderStatusId(value))
            .HasColumnName("WorkOrderStatusId")
            .IsRequired();

        builder.Property(x => x.WorkPriorityId)
            .HasConversion(id => id.Value, value => new WorkPriorityId(value))
            .HasColumnName("WorkPriorityId")
            .IsRequired();

        builder.Property(x => x.WorkOrderCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.RequestedAtUtc).HasColumnName("RequestedAtUtc").IsRequired();
        builder.Property(x => x.DueAtUtc);
        builder.Property(x => x.StartedAtUtc);
        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.ClosedAtUtc);
        builder.Property(x => x.RequestedByUserId).HasColumnName("RequestedByUserId");
        builder.Property(x => x.AssignedTeamId).HasColumnName("AssignedTeamId");
        builder.Property(x => x.AssignedPersonId).HasColumnName("AssignedPersonId");
        builder.Property(x => x.OriginOperationalEventId).HasColumnName("OriginOperationalEventId");
        builder.Property(x => x.OriginIncidentActionId).HasColumnName("OriginIncidentActionId");
        builder.Property(x => x.IsDeleted).HasColumnName("IsDeleted").IsRequired();

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.WorkOrderCode).IsUnique().HasDatabaseName("UQ_Maintenance_WorkOrder_Code");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_Maintenance_WorkOrder_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .HasConstraintName("FK_Maintenance_WorkOrder_Asset")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkOrderType>()
            .WithMany()
            .HasForeignKey(x => x.WorkOrderTypeId)
            .HasConstraintName("FK_Maintenance_WorkOrder_Type")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkOrderStatus>()
            .WithMany()
            .HasForeignKey(x => x.WorkOrderStatusId)
            .HasConstraintName("FK_Maintenance_WorkOrder_Status")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkPriority>()
            .WithMany()
            .HasForeignKey(x => x.WorkPriorityId)
            .HasConstraintName("FK_Maintenance_WorkOrder_Priority")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
