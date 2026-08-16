using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>
/// The atlas DDL (C.6.4.5) gives this table only CreatedAtUtc (no
/// RowVersion), verified directly against the DDL. ReviewedAtUtc is the
/// real business timestamp (when the review happened) and is
/// domain-modeled; CreatedAtUtc is pure row-insertion bookkeeping with a
/// SQL DEFAULT and is mapped as a shadow column only.
/// </summary>
public sealed class TwinDivergenceReviewConfiguration : IEntityTypeConfiguration<TwinDivergenceReview>
{
    public void Configure(EntityTypeBuilder<TwinDivergenceReview> builder)
    {
        builder.ToTable("TwinDivergenceReview", "DigitalTwin");
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_TwinDivergenceReview");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TwinDivergenceReviewId(value))
            .HasColumnName("TwinDivergenceReviewId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinDivergenceId)
            .HasConversion(id => id.Value, value => new TwinDivergenceId(value))
            .HasColumnName("TwinDivergenceId")
            .IsRequired();

        builder.Property(x => x.DivergenceStatusId)
            .HasConversion(id => id.Value, value => new DivergenceStatusId(value))
            .HasColumnName("DivergenceStatusId")
            .IsRequired();

        builder.Property(x => x.ReviewedByUserId).HasColumnName("ReviewedByUserId");
        builder.Property(x => x.ReviewedAtUtc).HasColumnName("ReviewedAtUtc").IsRequired();
        builder.Property(x => x.ReviewNote).HasColumnName("ReviewNote").HasMaxLength(2000);
        builder.Property(x => x.CorrectiveAction).HasColumnName("CorrectiveAction").HasMaxLength(1000);

        builder.Property<DateTime>("CreatedAtUtc").HasColumnName("CreatedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne<TwinDivergence>()
            .WithMany()
            .HasForeignKey(x => x.TwinDivergenceId)
            .HasConstraintName("FK_DigitalTwin_TwinDivergenceReview_TwinDivergence")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DivergenceStatus>()
            .WithMany()
            .HasForeignKey(x => x.DivergenceStatusId)
            .HasConstraintName("FK_DigitalTwin_TwinDivergenceReview_DivergenceStatus")
            .OnDelete(DeleteBehavior.Restrict);

        // ReviewedByUserId -> Security.ApplicationUser: no enforced FK, SecurityDb is a separate physical database (ADR-020).
        builder.Ignore(x => x.DomainEvents);
    }
}
