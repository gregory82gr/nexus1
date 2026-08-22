using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>Same shadow-property audit treatment as TwinModelConfiguration — see its XML doc for the full rationale.</summary>
public sealed class TwinVariableConfiguration : IEntityTypeConfiguration<TwinVariable>
{
    public void Configure(EntityTypeBuilder<TwinVariable> builder)
    {
        builder.ToTable("TwinVariable", "DigitalTwin", tb => tb.HasCheckConstraint(
            "CK_DigitalTwin_TwinVariable_Bounds", "[LowerBound] IS NULL OR [UpperBound] IS NULL OR [LowerBound] <= [UpperBound]"));
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinVariable");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinVariableId(value))
            .HasColumnName("TwinVariableId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinModelId)
            .HasConversion(id => id.Value, value => new TwinModelId(value))
            .HasColumnName("TwinModelId")
            .IsRequired();

        builder.Property(x => x.ModelVariableTypeId)
            .HasConversion(id => id.Value, value => new ModelVariableTypeId(value))
            .HasColumnName("ModelVariableTypeId")
            .IsRequired();

        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId");

        builder.Property(x => x.Code).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsStateVariable).IsRequired();
        builder.Property(x => x.IsRequired).IsRequired();
        builder.Property(x => x.LowerBound);
        builder.Property(x => x.UpperBound);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.TwinModelId, x.Code }).IsUnique().HasDatabaseName("UQ_DigitalTwin_TwinVariable_Model_Code");

        builder.HasOne<TwinModel>()
            .WithMany()
            .HasForeignKey(x => x.TwinModelId)
            .HasConstraintName("FK_DigitalTwin_TwinVariable_TwinModel")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ModelVariableType>()
            .WithMany()
            .HasForeignKey(x => x.ModelVariableTypeId)
            .HasConstraintName("FK_DigitalTwin_TwinVariable_ModelVariableType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_DigitalTwin_TwinVariable_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
