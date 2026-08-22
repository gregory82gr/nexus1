using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Configurations.ReinforcementLearning;

/// <summary>Full audit shape mapped as EF shadow properties only (ADR-026).</summary>
public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policy", "ReinforcementLearning");
        builder.HasKey(x => x.Id).HasName("PK_ReinforcementLearning_Policy");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PolicyId(value))
            .HasColumnName("PolicyId")
            .ValueGeneratedNever();

        builder.Property(x => x.QTableId)
            .HasConversion(id => id.Value, value => new QTableId(value))
            .HasColumnName("QTableId")
            .IsRequired();

        builder.Property(x => x.PolicyStatusId)
            .HasConversion(id => id.Value, value => new PolicyStatusId(value))
            .HasColumnName("PolicyStatusId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ExtractedAtUtc).IsRequired();
        builder.Property(x => x.EntryCount).IsRequired();

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReinforcementLearning_Policy_Code");

        builder.HasOne<QTable>()
            .WithMany()
            .HasForeignKey(x => x.QTableId)
            .HasConstraintName("FK_ReinforcementLearning_Policy_QTable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PolicyStatus>()
            .WithMany()
            .HasForeignKey(x => x.PolicyStatusId)
            .HasConstraintName("FK_ReinforcementLearning_Policy_PolicyStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
