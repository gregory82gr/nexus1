using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

public sealed class ValidationStatusConfiguration : IEntityTypeConfiguration<ValidationStatus>
{
    public void Configure(EntityTypeBuilder<ValidationStatus> builder)
    {
        builder.ToTable("ValidationStatus", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_ValidationStatus");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ValidationStatusId(value))
            .HasColumnName("ValidationStatusId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_DigitalTwin_ValidationStatus_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
