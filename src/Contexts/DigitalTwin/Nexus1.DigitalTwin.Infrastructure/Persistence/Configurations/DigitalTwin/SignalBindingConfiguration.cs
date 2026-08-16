using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>Same shadow-property audit treatment as TwinModelConfiguration — see its XML doc for the full rationale.</summary>
public sealed class SignalBindingConfiguration : IEntityTypeConfiguration<SignalBinding>
{
    public void Configure(EntityTypeBuilder<SignalBinding> builder)
    {
        builder.ToTable("SignalBinding", "DigitalTwin", tb => tb.HasCheckConstraint(
            "CK_DigitalTwin_SignalBinding_EffectiveRange", "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]"));
        builder.HasKey(x => x.Id).HasName("PK_DigitalTwin_SignalBinding");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SignalBindingId(value))
            .HasColumnName("SignalBindingId")
            .ValueGeneratedNever();

        builder.Property(x => x.TwinModelId)
            .HasConversion(id => id.Value, value => new TwinModelId(value))
            .HasColumnName("TwinModelId")
            .IsRequired();

        builder.Property(x => x.TwinVariableId)
            .HasConversion(id => id.Value, value => new TwinVariableId(value))
            .HasColumnName("TwinVariableId")
            .IsRequired();

        builder.Property(x => x.SignalId).HasColumnName("SignalId").IsRequired();

        builder.Property(x => x.BindingRoleId)
            .HasConversion(id => id.Value, value => new BindingRoleId(value))
            .HasColumnName("BindingRoleId")
            .IsRequired();

        builder.Property(x => x.BindingStatusId)
            .HasConversion(id => id.Value, value => new BindingStatusId(value))
            .HasColumnName("BindingStatusId")
            .IsRequired();

        builder.Property(x => x.ModelVariable).HasMaxLength(160).IsRequired();
        builder.Property(x => x.ScaleFactor).HasColumnType("decimal(18,8)");
        builder.Property(x => x.OffsetValue).HasColumnType("decimal(18,8)");
        builder.Property(x => x.ToleranceAbs);
        builder.Property(x => x.TolerancePercent);
        builder.Property(x => x.EffectiveFromUtc).IsRequired();
        builder.Property(x => x.EffectiveToUtc);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<string>("CreatedBy").HasColumnName("CreatedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<DateTime>("ModifiedAtUtc").HasColumnName("ModifiedAtUtc").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property<string>("ModifiedBy").HasColumnName("ModifiedBy").HasMaxLength(100).IsRequired().HasDefaultValueSql("N'system'");
        builder.Property<bool>("IsDeleted").HasColumnName("IsDeleted").IsRequired().HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.TwinModelId, x.TwinVariableId, x.SignalId }).IsUnique()
            .HasDatabaseName("UQ_DigitalTwin_SignalBinding_Model_Variable_Signal");

        builder.HasOne<TwinModel>()
            .WithMany()
            .HasForeignKey(x => x.TwinModelId)
            .HasConstraintName("FK_DigitalTwin_SignalBinding_TwinModel")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TwinVariable>()
            .WithMany()
            .HasForeignKey(x => x.TwinVariableId)
            .HasConstraintName("FK_DigitalTwin_SignalBinding_TwinVariable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<InstrumentationSignalReference>()
            .WithMany()
            .HasForeignKey(x => x.SignalId)
            .HasConstraintName("FK_DigitalTwin_SignalBinding_Signal")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BindingRole>()
            .WithMany()
            .HasForeignKey(x => x.BindingRoleId)
            .HasConstraintName("FK_DigitalTwin_SignalBinding_BindingRole")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BindingStatus>()
            .WithMany()
            .HasForeignKey(x => x.BindingStatusId)
            .HasConstraintName("FK_DigitalTwin_SignalBinding_BindingStatus")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
