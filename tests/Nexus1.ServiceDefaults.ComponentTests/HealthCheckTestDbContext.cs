using Microsoft.EntityFrameworkCore;

namespace Nexus1.ServiceDefaults.ComponentTests;

/// <summary>
/// Minimal, standalone DbContext used only to exercise
/// <see cref="DbContextHealthCheck{TContext}"/> against a real database —
/// deliberately decoupled from every business context so this test doesn't
/// need to reference any of them.
/// </summary>
public sealed class HealthCheckTestDbContext(DbContextOptions<HealthCheckTestDbContext> options) : DbContext(options)
{
    public DbSet<HealthCheckTestEntity> TestEntities => Set<HealthCheckTestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HealthCheckTestEntity>(entity =>
        {
            entity.ToTable("HealthCheckTestEntity", "dbo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });
    }
}

public sealed class HealthCheckTestEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
