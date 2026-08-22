using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>Full audit shape mapped as EF shadow properties only (ADR-026).</summary>
public sealed class StateSpaceConfiguration : IEntityTypeConfiguration<StateSpace>
{
    public void Configure(EntityTypeBuilder<StateSpace> builder)
    {
        builder.ToTable("StateSpace", "ReinforcementLearning", t => t.HasCheckConstraint(
            "CK_ReinforcementLearning_StateSpace_DimensionCount", "[DimensionCount] > 0"));
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_StateSpace");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new StateSpaceId(value))
            .HasColumnName("StateSpaceId")
            .ValueGeneratedNever();

        builder.Property(x => x.StateSpaceTypeId)
            .HasConversion(id => id.Value, value => new StateSpaceTypeId(value))
            .HasColumnName("StateSpaceTypeId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DimensionCount).IsRequired();
        builder.Property(x => x.IsDiscrete).IsRequired().HasDefaultValue(true);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_StateSpace_Code");

        builder.HasOne<StateSpaceType>()
            .WithMany()
            .HasForeignKey(x => x.StateSpaceTypeId)
            .HasConstraintName("FK_ReinforcementLearning_StateSpace_StateSpaceType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
