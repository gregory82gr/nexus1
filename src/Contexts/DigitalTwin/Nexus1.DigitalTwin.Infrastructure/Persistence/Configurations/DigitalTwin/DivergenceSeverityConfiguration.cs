using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

public sealed class DivergenceSeverityConfiguration : IEntityTypeConfiguration<DivergenceSeverity>
{
    public void Configure(EntityTypeBuilder<DivergenceSeverity> builder)
    {
        builder.ToTable("DivergenceSeverity", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_DivergenceSeverity");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DivergenceSeverityId(value))
            .HasColumnName("DivergenceSeverityId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_DigitalTwin_DivergenceSeverity_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
