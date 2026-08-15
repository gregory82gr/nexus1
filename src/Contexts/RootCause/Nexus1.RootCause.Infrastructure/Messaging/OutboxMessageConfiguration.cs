using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus1.RootCause.Infrastructure.Messaging;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessage", "messaging");
        builder.HasKey(x => x.MessageId).HasName("PK_messaging_OutboxMessage");

        builder.Property(x => x.MessageId).HasColumnName("MessageId").ValueGeneratedNever();
        builder.Property(x => x.Producer).HasMaxLength(80).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.RoutingKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.StoredAtUtc).IsRequired();
        builder.Property(x => x.EnvelopeBytes).HasColumnType("varbinary(max)").IsRequired();
        builder.Property(x => x.EnvelopeSha256).HasColumnType("binary(32)").IsRequired();
        builder.Property(x => x.ProcessedAtUtc);

        builder.Property(x => x.TraceId).HasMaxLength(32);
        builder.Property(x => x.SpanId).HasMaxLength(16);
        builder.Property(x => x.TraceFlags);
        builder.Property(x => x.TraceState).HasMaxLength(512);

        builder.HasIndex(x => x.ProcessedAtUtc).HasDatabaseName("IX_messaging_OutboxMessage_ProcessedAtUtc");
    }
}
