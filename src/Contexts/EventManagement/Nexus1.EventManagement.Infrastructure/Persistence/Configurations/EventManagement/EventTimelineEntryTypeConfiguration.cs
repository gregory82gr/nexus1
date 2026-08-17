using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

public sealed class EventTimelineEntryTypeConfiguration : IEntityTypeConfiguration<EventTimelineEntryType>
{
    public void Configure(EntityTypeBuilder<EventTimelineEntryType> builder)
    {
        builder.ToTable("EventTimelineEntryType", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_EventTimelineEntryType");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new EventTimelineEntryTypeId(value))
            .HasColumnName("EventTimelineEntryTypeId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EventManagement_EventTimelineEntryType_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
