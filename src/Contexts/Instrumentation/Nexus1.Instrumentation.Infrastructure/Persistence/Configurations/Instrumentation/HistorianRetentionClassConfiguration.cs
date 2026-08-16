using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

public sealed class HistorianRetentionClassConfiguration : IEntityTypeConfiguration<HistorianRetentionClass>
{
    public void Configure(EntityTypeBuilder<HistorianRetentionClass> builder)
    {
        builder.ToTable("HistorianRetentionClass", "Instrumentation");
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_HistorianRetentionClass");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new HistorianRetentionClassId(value))
            .HasColumnName("HistorianRetentionClassId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Instrumentation_HistorianRetentionClass_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
