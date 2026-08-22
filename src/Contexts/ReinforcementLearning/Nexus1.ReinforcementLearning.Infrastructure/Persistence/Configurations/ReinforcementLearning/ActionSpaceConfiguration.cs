using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>
/// Full audit shape mapped as EF shadow properties only (ADR-026).
/// EngineeringUnitId is a real, nullable FK to CorePlatform.EngineeringUnit
/// via the shadow-entity technique.
/// </summary>
public sealed class ActionSpaceConfiguration : IEntityTypeConfiguration<ActionSpace>
{
    public void Configure(EntityTypeBuilder<ActionSpace> builder)
    {
        builder.ToTable("ActionSpace", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_ActionSpace");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ActionSpaceId(value))
            .HasColumnName("ActionSpaceId")
            .ValueGeneratedNever();

        builder.Property(x => x.ActionSpaceTypeId)
            .HasConversion(id => id.Value, value => new ActionSpaceTypeId(value))
            .HasColumnName("ActionSpaceTypeId")
            .IsRequired();

        builder.Property(x => x.EngineeringUnitId).HasColumnName("EngineeringUnitId");

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_ActionSpace_Code");

        builder.HasOne<ActionSpaceType>()
            .WithMany()
            .HasForeignKey(x => x.ActionSpaceTypeId)
            .HasConstraintName("FK_ReinforcementLearning_ActionSpace_ActionSpaceType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorePlatformEngineeringUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.EngineeringUnitId)
            .HasConstraintName("FK_RL_ActionSpace_EngineeringUnit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
