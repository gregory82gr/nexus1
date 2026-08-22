using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

/// <summary>Shares AlarmManagementDb's physical database (ADR-015, following ADR-006's precedent) but keeps its own migration history and only knows about CorePlatform.* tables.</summary>
public sealed class CorePlatformDbContext(DbContextOptions<CorePlatformDbContext> options) : DbContext(options)
{
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();

    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    public DbSet<Language> Languages => Set<Language>();

    public DbSet<Localization> Localizations => Set<Localization>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Region> Regions => Set<Region>();

    public DbSet<TimeZoneReference> TimeZones => Set<TimeZoneReference>();

    public DbSet<Calendar> Calendars => Set<Calendar>();

    public DbSet<EngineeringUnit> EngineeringUnits => Set<EngineeringUnit>();

    public DbSet<DeploymentVersion> DeploymentVersions => Set<DeploymentVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CorePlatformDbContext).Assembly);
    }
}
