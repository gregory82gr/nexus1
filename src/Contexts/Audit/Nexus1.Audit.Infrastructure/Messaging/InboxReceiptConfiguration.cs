using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.Audit.Infrastructure.Messaging;

public sealed class InboxReceiptConfiguration : IEntityTypeConfiguration<InboxReceipt>
{
    public void Configure(EntityTypeBuilder<InboxReceipt> builder)
    {
        builder.ToTable("InboxReceipt", "messaging");
        builder.HasKey(x => new { x.ConsumerName, x.MessageId }).HasName("PK_messaging_InboxReceipt");

        builder.Property(x => x.ConsumerName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.Producer).HasMaxLength(80).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).IsRequired();
        builder.Property(x => x.CompletedAtUtc).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_messaging_InboxReceipt_CompletedAtUtc", "[CompletedAtUtc] >= [ReceivedAtUtc]"));
    }
}
