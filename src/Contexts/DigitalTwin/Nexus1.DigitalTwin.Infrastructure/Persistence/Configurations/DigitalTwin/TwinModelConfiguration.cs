using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>
/// TwinModel's atlas DDL (C.6.4.3) carries the full audit-column shape
/// (CreatedBy, ModifiedAtUtc, ModifiedBy, IsDeleted, RowVersion) — unlike
/// most tables built in prior sectors, where Domain's "audit columns not
/// modeled" restraint was possible because the atlas simply never gave
/// those tables the columns to begin with. Here they are real NOT NULL
/// columns, so Infrastructure maps them as EF shadow properties (never
/// exposed on the Domain type) rather than dropping them from the physical
/// schema entirely.
///
/// Named, deliberate discrepancy against the atlas's literal DDL: CreatedBy
/// and ModifiedBy are NVARCHAR(100) NOT NULL with NO SQL DEFAULT in the
/// atlas (unlike CreatedAtUtc/ModifiedAtUtc/IsDeleted, which do have
/// DEFAULT constraints) — no prior sector needed to solve this because no
/// prior in-scope table combined "audit columns excluded from Domain" with
/// "NOT NULL, no default" for a string column. Since this project is
/// Code-First (the physical table is generated FROM this model, not
/// scaffolded from a pre-existing one), leaving CreatedBy/ModifiedBy
/// entirely unmapped would drop the columns from the migration outright,
/// and mapping them with no default would make every insert fail its own
/// NOT NULL constraint (no app code ever sets a shadow property it doesn't
/// know exists). The chosen fix: map them as shadow properties with
/// HasDefaultValueSql("N'system'") — a DEFAULT CONSTRAINT the atlas's own
/// text does not specify, added solely so the atlas's own NOT NULL
/// constraint is satisfiable without inventing an audit-trail feature this
/// phase was never asked to build. Flagged explicitly in the session's
/// final report rather than silently guessed.
/// </summary>
public sealed class TwinModelConfiguration : IEntityTypeConfiguration<TwinModel>
{
    public void Configure(EntityTypeBuilder<TwinModel> builder)
    {
        builder.ToTable("TwinModel", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinModel");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinModelId(value))
            .HasColumnName("TwinModelId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId).HasColumnName("UnitId").IsRequired();

        builder.Property(x => x.TwinModelTypeId)
            .HasConversion(id => id.Value, value => new TwinModelTypeId(value))
            .HasColumnName("TwinModelTypeId")
            .IsRequired();

        builder.Property(x => x.TwinModelStatusId)
            .HasConversion(id => id.Value, value => new TwinModelStatusId(value))
            .HasColumnName("TwinModelStatusId")
            .IsRequired();

        builder.Property(x => x.TwinFidelityLevelId)
            .HasConversion(id => id.Value, value => new TwinFidelityLevelId(value))
            .HasColumnName("TwinFidelityLevelId")
            .IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ModelOwner).HasMaxLength(200);
        builder.Property(x => x.IsAuthoritative).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.UnitId, x.Code }).IsUnique().HasDatabaseName("UQ_DigitalTwin_TwinModel_Unit_Code");

        builder.HasOne<ReactorFleetUnitReference>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_DigitalTwin_TwinModel_Unit")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TwinModelType>()
            .WithMany()
            .HasForeignKey(x => x.TwinModelTypeId)
            .HasConstraintName("FK_DigitalTwin_TwinModel_TwinModelType")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TwinModelStatus>()
            .WithMany()
            .HasForeignKey(x => x.TwinModelStatusId)
            .HasConstraintName("FK_DigitalTwin_TwinModel_TwinModelStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TwinFidelityLevel>()
            .WithMany()
            .HasForeignKey(x => x.TwinFidelityLevelId)
            .HasConstraintName("FK_DigitalTwin_TwinModel_TwinFidelityLevel")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
