using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

public sealed class AcquisitionConnectionConfiguration : IEntityTypeConfiguration<AcquisitionConnection>
{
    public void Configure(EntityTypeBuilder<AcquisitionConnection> builder)
    {
        builder.ToTable("AcquisitionConnection", "Instrumentation", tb => tb.HasCheckConstraint(
            "CK_Instrumentation_AcquisitionConnection_PollInterval", "[PollIntervalMs] IS NULL OR [PollIntervalMs] > 0"));
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_AcquisitionConnection");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AcquisitionConnectionId(value))
            .HasColumnName("AcquisitionConnectionId")
            .ValueGeneratedNever();

        builder.Property(x => x.DataAcquisitionNodeId)
            .HasConversion(id => id.Value, value => new DataAcquisitionNodeId(value))
            .HasColumnName("DataAcquisitionNodeId")
            .IsRequired();

        builder.Property(x => x.ChannelStatusId)
            .HasConversion(id => id.Value, value => new ChannelStatusId(value))
            .HasColumnName("ChannelStatusId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Protocol).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Endpoint).HasMaxLength(500);
        builder.Property(x => x.PollIntervalMs);
        builder.Property(x => x.IsReadOnly).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.DataAcquisitionNodeId, x.Code }).IsUnique().HasDatabaseName("UQ_Instrumentation_AcquisitionConnection_Node_Code");

        builder.HasOne<DataAcquisitionNode>()
            .WithMany()
            .HasForeignKey(x => x.DataAcquisitionNodeId)
            .HasConstraintName("FK_Instrumentation_AcquisitionConnection_DataAcquisitionNode")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChannelStatus>()
            .WithMany()
            .HasForeignKey(x => x.ChannelStatusId)
            .HasConstraintName("FK_Instrumentation_AcquisitionConnection_ChannelStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
