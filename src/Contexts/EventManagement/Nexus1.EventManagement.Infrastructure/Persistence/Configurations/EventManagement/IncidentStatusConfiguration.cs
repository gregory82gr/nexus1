using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence.Configurations.EventManagement;

public sealed class IncidentStatusConfiguration : IEntityTypeConfiguration<IncidentStatus>
{
    public void Configure(EntityTypeBuilder<IncidentStatus> builder)
    {
        builder.ToTable("IncidentStatus", "EventManagement");
        builder.HasKey(x => x.Id).HasName("PK_EventManagement_IncidentStatus");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new IncidentStatusId(value))
            .HasColumnName("IncidentStatusId")
            .ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_EventManagement_IncidentStatus_Code");

        builder.Ignore(x => x.DomainEvents);
    }
}
