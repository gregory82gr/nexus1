using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

public sealed class EngineeringUnitConfiguration : IEntityTypeConfiguration<EngineeringUnit>
{
    public void Configure(EntityTypeBuilder<EngineeringUnit> builder)
    {
        // Atlas C.1.4.8, verbatim: the fourteen QuantityType values match
        // EngineeringQuantityType's own member names exactly, so this
        // constraint was always compatible with the existing
        // HasConversion<string>() mapping below — its absence (found
        // during the CorePlatform slice's own QuantityType-mismatch
        // investigation) was a doc-vs-implementation gap, not a design
        // conflict. Added now; both real rows already carry corrected,
        // matching values.
        builder.ToTable("EngineeringUnit", "CorePlatform", t => t.HasCheckConstraint(
            "CK_CorePlatform_EngineeringUnit_QuantityType",
            "[QuantityType] IN (N'Power', N'ThermalPower', N'Temperature', N'Pressure', N'Reactivity', " +
            "N'Flow', N'Frequency', N'Percentage', N'RadiationDoseRate', N'RadiationDose', N'CountRate', " +
            "N'Mass', N'Time', N'Other')"));
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_EngineeringUnit");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EngineeringUnitId(value))
            .HasColumnName("EngineeringUnitId")
            .ValueGeneratedNever();

        builder.Property(x => x.Symbol).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.QuantityType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.SiConversionFactor).HasColumnType("decimal(28,12)");
        builder.Property(x => x.SiConversionOffset).HasColumnType("decimal(28,12)");
        builder.Property(x => x.IsDimensionless).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Symbol).IsUnique().HasDatabaseName("UQ_CorePlatform_EngineeringUnit_Symbol");
        builder.HasIndex(x => x.QuantityType).HasDatabaseName("IX_CorePlatform_EngineeringUnit_QuantityType");

        builder.Ignore(x => x.DomainEvents);
    }
}
