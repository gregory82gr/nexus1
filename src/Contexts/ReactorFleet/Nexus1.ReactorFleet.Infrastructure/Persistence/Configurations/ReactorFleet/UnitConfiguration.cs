using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.Infrastructure.Persistence.Configurations.ReactorFleet;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Unit", "ReactorFleet");
        builder.HasKey(x => x.Id).HasName("PK_ReactorFleet_Unit");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new UnitId(value))
            .HasColumnName("UnitId")
            // Atlas declares this IDENTITY; not used here because Unit.Create
            // always requires a caller-supplied id (consistent with every
            // aggregate factory already built across all three contexts).
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_ReactorFleet_Unit_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
