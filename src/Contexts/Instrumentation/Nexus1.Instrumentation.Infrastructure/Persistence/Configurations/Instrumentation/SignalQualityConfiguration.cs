using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

public sealed class SignalQualityConfiguration : IEntityTypeConfiguration<SignalQuality>
{
    public void Configure(EntityTypeBuilder<SignalQuality> builder)
    {
        builder.ToTable("SignalQuality", "Instrumentation");
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_SignalQuality");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SignalQualityId(value))
            .HasColumnName("SignalQualityId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Instrumentation_SignalQuality_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
