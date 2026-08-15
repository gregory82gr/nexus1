using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence.Configurations.AlarmManagement;

public sealed class AlarmFloodConfiguration : IEntityTypeConfiguration<AlarmFlood>
{
    public void Configure(EntityTypeBuilder<AlarmFlood> builder)
    {
        builder.ToTable("AlarmFlood", "AlarmManagement");
        builder.HasKey(x => x.Id).HasName("PK_AlarmManagement_AlarmFlood");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AlarmFloodId(value))
            .HasColumnName("AlarmFloodId")
            .ValueGeneratedNever();

        builder.Property(x => x.UnitId)
            .HasConversion(id => id.Value, value => new UnitId(value))
            .HasColumnName("UnitId")
            .IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.EndedAtUtc);

        builder.HasIndex(x => new { x.UnitId, x.StartedAtUtc }).HasDatabaseName("IX_AlarmManagement_AlarmFlood_UnitId_StartedAtUtc");

        // AlarmFloodMember (the atlas's join table for flood membership) is
        // explicitly deferred per ADR-004 — MemberAlarmEventIds is in-memory
        // only until that table is built.
        builder.Ignore(x => x.MemberAlarmEventIds);
        builder.Ignore(x => x.DomainEvents);
    }
}
