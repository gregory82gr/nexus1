using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Reporting.Domain;

namespace Nexus1.Reporting.Infrastructure.Persistence.Configurations.Reporting;

public sealed class RootCauseCaseSummaryConfiguration : IEntityTypeConfiguration<RootCauseCaseSummary>
{
    public void Configure(EntityTypeBuilder<RootCauseCaseSummary> builder)
    {
        builder.ToTable("RootCauseCaseSummary", "Reporting");
        builder.HasKey(x => x.Id).HasName("PK_Reporting_RootCauseCaseSummary");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new RootCauseCaseSummaryId(value))
            .HasColumnName("RootCauseCaseSummaryId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).IsRequired();
        builder.Property(x => x.AlarmFloodId).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Verdict).HasMaxLength(1000);
        builder.Property(x => x.OpenedAtUtc).IsRequired();
        builder.Property(x => x.VerdictIssuedAtUtc);
        builder.Property(x => x.LastAppliedAtUtc).IsRequired();
        builder.Property(x => x.LastAppliedMessageId).IsRequired();

        builder.Ignore(x => x.DomainEvents);
    }
}
