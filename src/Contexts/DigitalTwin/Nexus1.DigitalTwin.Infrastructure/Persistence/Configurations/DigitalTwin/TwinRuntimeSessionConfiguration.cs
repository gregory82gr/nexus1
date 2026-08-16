using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>
/// Leaner audit shape than TwinModel/TwinModelVersion/TwinVariable/SignalBinding
/// — the atlas DDL (C.6.4.4) gives this table only CreatedAtUtc + CreatedBy +
/// RowVersion (no ModifiedAtUtc/ModifiedBy/IsDeleted), verified directly
/// against the DDL. CreatedBy still needs the same shadow-property
/// HasDefaultValueSql("N'system'") treatment as TwinModelConfiguration —
/// see its XML doc for the full rationale.
/// </summary>
public sealed class TwinRuntimeSessionConfiguration : IEntityTypeConfiguration<TwinRuntimeSession>
{
    public void Configure(EntityTypeBuilder<TwinRuntimeSession> builder)
    {
        builder.ToTable("TwinRuntimeSession", "DigitalTwin", tb => tb.HasCheckConstraint(
            "CK_DigitalTwin_TwinRuntimeSession_TimeRange", "[EndedAtUtc] IS NULL OR [EndedAtUtc] > [StartedAtUtc]"));
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinRuntimeSession");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinRuntimeSessionId(value))
            .HasColumnName("TwinRuntimeSessionId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinModelVersionId)
            .HasConversion(id => id.Value, value => new TwinModelVersionId(value))
            .HasColumnName("TwinModelVersionId")
            .IsRequired();

        builder.Property(x => x.StartedByUserId).HasColumnName("StartedByUserId");

        builder.Property(x => x.SessionCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RuntimeMode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.HostName).HasMaxLength(250);
        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.EndedAtUtc);
        builder.Property(x => x.IsReadOnly).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.SessionCode).IsUnique().HasDatabaseName("UQ_DigitalTwin_TwinRuntimeSession_Code");

        builder.HasOne<TwinModelVersion>()
            .WithMany()
            .HasForeignKey(x => x.TwinModelVersionId)
            .HasConstraintName("FK_DigitalTwin_TwinRuntimeSession_TwinModelVersion")
            .OnDelete(DeleteBehavior.Restrict);

        // StartedByUserId -> Security.ApplicationUser: no enforced FK, SecurityDb is a separate physical database (ADR-020).
        builder.Ignore(x => x.DomainEvents);
    }
}
