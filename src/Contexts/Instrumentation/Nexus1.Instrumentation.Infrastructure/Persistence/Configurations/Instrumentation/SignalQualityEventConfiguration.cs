using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Configurations.Instrumentation;

public sealed class SignalQualityEventConfiguration : IEntityTypeConfiguration<SignalQualityEvent>
{
    public void Configure(EntityTypeBuilder<SignalQualityEvent> builder)
    {
        builder.ToTable("SignalQualityEvent", "Instrumentation", tb => tb.HasCheckConstraint(
            "CK_Instrumentation_SignalQualityEvent_Time", "[EndedAtUtc] IS NULL OR [EndedAtUtc] > [StartedAtUtc]"));
        builder.HasKey(x => x.Id).HasName("PK_Instrumentation_SignalQualityEvent");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SignalQualityEventId(value))
            .HasColumnName("SignalQualityEventId")
            .ValueGeneratedNever();

        builder.Property(x => x.SignalId)
            .HasConversion(id => id.Value, value => new SignalId(value))
            .HasColumnName("SignalId")
            .IsRequired();

        builder.Property(x => x.SignalQualityId)
            .HasConversion(id => id.Value, value => new SignalQualityId(value))
            .HasColumnName("SignalQualityId")
            .IsRequired();

        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.EndedAtUtc);
        builder.Property(x => x.ReasonCode).HasMaxLength(80);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.SignalId, x.EndedAtUtc }).HasDatabaseName("IX_Instrumentation_SignalQualityEvent_SignalId_EndedAtUtc");

        builder.HasOne<Signal>()
            .WithMany()
            .HasForeignKey(x => x.SignalId)
            .HasConstraintName("FK_Instrumentation_SignalQualityEvent_Signal")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SignalQuality>()
            .WithMany()
            .HasForeignKey(x => x.SignalQualityId)
            .HasConstraintName("FK_Instrumentation_SignalQualityEvent_SignalQuality")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
