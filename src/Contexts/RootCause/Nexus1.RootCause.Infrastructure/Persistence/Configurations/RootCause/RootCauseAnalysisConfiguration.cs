using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.RootCause;

/// <summary>
/// DeleteBehavior.Restrict on Hypotheses matches the book's own
/// RootCauseCaseConfiguration rationale (Ch.23 p.78): a case should not
/// silently lose its hypotheses to an accidental cascade.
/// </summary>
public sealed class RootCauseAnalysisConfiguration : IEntityTypeConfiguration<RootCauseAnalysis>
{
    public void Configure(EntityTypeBuilder<RootCauseAnalysis> builder)
    {
        builder.ToTable("RootCauseAnalysis", "RootCause");
        builder.HasKey(x => x.Id).HasName("PK_RootCause_RootCauseAnalysis");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RootCauseAnalysisId(value))
            .HasColumnName("RootCauseAnalysisId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId)
            .HasConversion(id => id.Value, value => new UnitId(value))
            .HasColumnName("UnitId")
            .IsRequired();

        builder.Property(x => x.AlarmFloodId)
            .HasConversion(id => id.Value, value => new AlarmFloodId(value))
            .HasColumnName("AlarmFloodId")
            .IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.OpenedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OpenedAtUtc).IsRequired();
        builder.Property(x => x.Verdict).HasMaxLength(1000);
        builder.Property(x => x.ClosedBy).HasMaxLength(100);
        builder.Property(x => x.ClosedAtUtc);

        builder.HasMany(x => x.Hypotheses)
            .WithOne()
            .HasForeignKey("RootCauseAnalysisId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RootCause_AnalysisHypothesis_RootCauseAnalysisId");
        builder.Navigation(x => x.Hypotheses).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Not unique: neither source states one analysis per flood is a rule,
        // and the atlas's own AlarmFloodId FK is nullable (other origins exist).
        builder.HasIndex(x => x.AlarmFloodId).HasDatabaseName("IX_RootCause_RootCauseAnalysis_AlarmFloodId");

        builder.Ignore(x => x.DomainEvents);
    }
}
