using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

public sealed class AcquisitionPointConfiguration : IEntityTypeConfiguration<AcquisitionPoint>
{
    public void Configure(EntityTypeBuilder<AcquisitionPoint> builder)
    {
        builder.ToTable("AcquisitionPoint", "Instrumentation");
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_AcquisitionPoint");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AcquisitionPointId(value))
            .HasColumnName("AcquisitionPointId")
            .ValueGeneratedNever();

        builder.Property(x => x.AcquisitionConnectionId)
            .HasConversion(id => id.Value, value => new AcquisitionConnectionId(value))
            .HasColumnName("AcquisitionConnectionId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RawAddress).HasMaxLength(300).IsRequired();
        builder.Property(x => x.RawDataType).HasMaxLength(80);
        builder.Property(x => x.ScaleFactor).HasColumnType("decimal(18,8)");
        builder.Property(x => x.OffsetValue).HasColumnType("decimal(18,8)");
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.AcquisitionConnectionId, x.Code }).IsUnique().HasDatabaseName("UQ_Instrumentation_AcquisitionPoint_Connection_Code");

        builder.HasOne<AcquisitionConnection>()
            .WithMany()
            .HasForeignKey(x => x.AcquisitionConnectionId)
            .HasConstraintName("FK_Instrumentation_AcquisitionPoint_AcquisitionConnection")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
