using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Configurations.CorePlatform;

/// <summary>Maps the C# DeploymentVersion type to the atlas's own CorePlatform.Version table name (see DeploymentVersion's own doc comment for why the C# name differs).</summary>
public sealed class DeploymentVersionConfiguration : IEntityTypeConfiguration<DeploymentVersion>
{
    public void Configure(EntityTypeBuilder<DeploymentVersion> builder)
    {
        builder.ToTable("Version", "CorePlatform");
        builder.HasKey(x => x.Id).HasName("PK_CorePlatform_Version");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DeploymentVersionId(value))
            .HasColumnName("VersionId")
            .ValueGeneratedNever();

        builder.Property(x => x.ComponentName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ComponentType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.VersionNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BuildSignature).HasMaxLength(100);
        builder.Property(x => x.GitCommit).HasMaxLength(40).IsFixedLength();
        builder.Property(x => x.SchemaMigration).HasMaxLength(150);
        builder.Property(x => x.ReleaseDateUtc);
        builder.Property(x => x.ChangelogSummary).HasMaxLength(1000);
        builder.Property(x => x.IsCurrent).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.ComponentName, x.VersionNumber })
            .IsUnique()
            .HasDatabaseName("UQ_CorePlatform_Version_Component_Version");

        builder.HasIndex(x => x.ComponentName)
            .IsUnique()
            .HasFilter("[IsCurrent] = 1")
            .HasDatabaseName("UX_CorePlatform_Version_Current_Component");

        builder.Ignore(x => x.DomainEvents);
    }
}
