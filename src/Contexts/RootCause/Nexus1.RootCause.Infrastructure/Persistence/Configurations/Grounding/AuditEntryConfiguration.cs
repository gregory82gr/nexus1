using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.Grounding;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("Audit", "RootCause");
        builder.HasKey(x => x.Seq).HasName("PK_RootCause_Audit");

        builder.Property(x => x.Seq).ValueGeneratedOnAdd();
        builder.Property(x => x.TimestampUtc).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.PrevHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.Hash).HasMaxLength(64).IsFixedLength().IsRequired();
    }
}
