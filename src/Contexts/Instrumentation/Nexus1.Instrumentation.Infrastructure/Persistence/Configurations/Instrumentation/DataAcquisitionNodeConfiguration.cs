using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

public sealed class DataAcquisitionNodeConfiguration : IEntityTypeConfiguration<DataAcquisitionNode>
{
    public void Configure(EntityTypeBuilder<DataAcquisitionNode> builder)
    {
        builder.ToTable("DataAcquisitionNode", "Instrumentation");
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_DataAcquisitionNode");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DataAcquisitionNodeId(value))
            .HasColumnName("DataAcquisitionNodeId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();

        builder.Property(x => x.ChannelStatusId)
            .HasConversion(id => id.Value, value => new ChannelStatusId(value))
            .HasColumnName("ChannelStatusId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.HostName).HasMaxLength(250);
        builder.Property(x => x.NetworkZone).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.UnitId, x.Code }).IsUnique().HasDatabaseName("UQ_Instrumentation_DataAcquisitionNode_Unit_Code");

        // Real SQL FOREIGN KEY to ReactorFleet.Unit (ADR-019) via the local
        // shadow reference type — see ReactorFleetUnitReference's own doc
        // comment for why this technique is used instead of a cross-context
        // ProjectReference.
        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_Instrumentation_DataAcquisitionNode_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChannelStatus>()
            .WithMany()
            .HasForeignKey(x => x.ChannelStatusId)
            .HasConstraintName("FK_Instrumentation_DataAcquisitionNode_ChannelStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
