using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence.Configurations.Organization;

/// <summary>
/// CreatedByUserId is a plain passport column with no FK constraint —
/// Security lives in its own SecurityDb while Organization gets its own
/// OrganizationDb, so a real cross-database FOREIGN KEY is not possible
/// (ADR-017).
/// </summary>
public sealed class StaffingScenarioConfiguration : IEntityTypeConfiguration<StaffingScenario>
{
    public void Configure(EntityTypeBuilder<StaffingScenario> builder)
    {
        builder.ToTable("StaffingScenario", "Organization");
        builder.HasKey(x => x.Id).HasName("PK_Organization_StaffingScenario");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new StaffingScenarioId(value))
            .HasColumnName("StaffingScenarioId")
            .ValueGeneratedNever();

        builder.Property(x => x.SiteId)
            .HasConversion(id => id.Value, value => new SiteId(value))
            .HasColumnName("SiteId")
            .IsRequired();

        builder.Property(x => x.ScenarioCode).HasColumnName("ScenarioCode").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedByUserId).HasColumnName("CreatedByUserId");
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.SiteId, x.ScenarioCode }).IsUnique().HasDatabaseName("UQ_Organization_StaffingScenario_Site_Code");

        builder.HasOne<Site>().WithMany().HasForeignKey(x => x.SiteId)
            .HasConstraintName("FK_Organization_StaffingScenario_Site")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
