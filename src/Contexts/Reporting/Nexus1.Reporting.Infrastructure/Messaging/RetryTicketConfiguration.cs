using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Reporting.Infrastructure.Messaging;

public sealed class RetryTicketConfiguration : IEntityTypeConfiguration<RetryTicket>
{
    public void Configure(EntityTypeBuilder<RetryTicket> builder)
    {
        builder.ToTable("RetryTicket", "messaging");
        builder.HasKey(x => x.RetryTicketId).HasName("PK_messaging_RetryTicket");

        builder.Property(x => x.RetryTicketId).ValueGeneratedNever();
        builder.Property(x => x.ConsumerName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.Attempt).IsRequired();
        builder.Property(x => x.PolicyId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FailureCode).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FirstFailedAtUtc).IsRequired();
        builder.Property(x => x.DueAtUtc).IsRequired();
        builder.Property(x => x.Producer).HasMaxLength(80).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.OriginalRoutingKey).HasMaxLength(240).IsRequired();
        builder.Property(x => x.EnvelopeBytes).HasColumnType("varbinary(max)").IsRequired();
        builder.Property(x => x.EnvelopeSha256).HasColumnType("binary(32)").IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.PublishedAtUtc);

        builder.HasIndex(x => new { x.ConsumerName, x.MessageId, x.Attempt })
            .IsUnique()
            .HasDatabaseName("UQ_messaging_RetryTicket_Attempt");

        builder.HasIndex(x => new { x.PublishedAtUtc, x.DueAtUtc })
            .HasDatabaseName("IX_messaging_RetryTicket_Due");
    }
}
