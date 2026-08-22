using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>
/// Full audit shape mapped as EF shadow properties only (ADR-026). UnitId
/// is a real FK to ReactorFleet.Unit; TwinModelId is a real, nullable FK to
/// DigitalTwin.TwinModel — both via the shadow-entity technique.
/// </summary>
public sealed class EnvironmentModelConfiguration : IEntityTypeConfiguration<EnvironmentModel>
{
    public void Configure(EntityTypeBuilder<EnvironmentModel> builder)
    {
        builder.ToTable("EnvironmentModel", "ReinforcementLearning", t => t.HasCheckConstraint(
            "CK_ReinforcementLearning_EnvironmentModel_TimeStepSeconds", "[TimeStepSeconds] > 0"));
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_EnvironmentModel");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EnvironmentModelId(value))
            .HasColumnName("EnvironmentModelId")
            .ValueGeneratedNever();

        builder.Property(x => x.EnvironmentModelTypeId)
            .HasConversion(id => id.Value, value => new EnvironmentModelTypeId(value))
            .HasColumnName("EnvironmentModelTypeId")
            .IsRequired();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();
        builder.Property(x => x.TwinModelId).HasColumnName("TwinModelId");

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.VersionLabel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TimeStepSeconds).HasColumnType("decimal(10,4)").IsRequired();
        builder.Property(x => x.IsDeterministic).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.RandomSeed);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_EnvironmentModel_Code");

        builder.HasOne<EnvironmentModelType>()
            .WithMany()
            .HasForeignKey(x => x.EnvironmentModelTypeId)
            .HasConstraintName("FK_ReinforcementLearning_EnvironmentModel_EnvironmentModelType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_RL_EnvironmentModel_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DigitalTwinTwinModelReference>()
            .WithMany()
            .HasForeignKey(x => x.TwinModelId)
            .HasConstraintName("FK_RL_EnvironmentModel_TwinModel")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
