using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.RootCause.Infrastructure.Messaging;

public sealed class PoisonMessageConfiguration : IEntityTypeConfiguration<PoisonMessage>
{
    public void Configure(EntityTypeBuilder<PoisonMessage> builder)
    {
        builder.ToTable("PoisonMessage", "messaging");
        builder.HasKey(x => x.PoisonMessageId).HasName("PK_messaging_PoisonMessage");

        builder.Property(x => x.PoisonMessageId).ValueGeneratedNever();
        builder.Property(x => x.ConsumerName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.EnvelopeSha256).HasColumnType("binary(32)").IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.TerminalReason).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RetryAttempts).IsRequired();
        builder.Property(x => x.FirstFailedAtUtc).IsRequired();
        builder.Property(x => x.QuarantinedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.ConsumerName, x.MessageId })
            .IsUnique()
            .HasDatabaseName("UQ_messaging_PoisonMessage_Identity");
    }
}
