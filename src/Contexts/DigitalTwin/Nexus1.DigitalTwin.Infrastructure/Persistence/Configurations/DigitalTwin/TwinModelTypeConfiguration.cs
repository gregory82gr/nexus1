using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

public sealed class TwinModelTypeConfiguration : IEntityTypeConfiguration<TwinModelType>
{
    public void Configure(EntityTypeBuilder<TwinModelType> builder)
    {
        builder.ToTable("TwinModelType", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinModelType");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinModelTypeId(value))
            .HasColumnName("TwinModelTypeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_DigitalTwin_TwinModelType_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
