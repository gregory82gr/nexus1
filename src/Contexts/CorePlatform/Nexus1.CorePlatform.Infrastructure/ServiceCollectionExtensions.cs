using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;

namespace Nexus1.CorePlatform.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-015 — CorePlatform shares that physical database, own schema, own migration history).</summary>
    public static IServiceCollection AddCorePlatformInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CorePlatformDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_CorePlatform")));

        services.AddScoped<IRepository<AppSetting, AppSettingId>, EfRepository<AppSetting, AppSettingId>>();
        services.AddScoped<IRepository<SystemConfiguration, SystemConfigurationId>, EfRepository<SystemConfiguration, SystemConfigurationId>>();
        services.AddScoped<IRepository<FeatureFlag, FeatureFlagId>, EfRepository<FeatureFlag, FeatureFlagId>>();
        services.AddScoped<IRepository<Language, LanguageId>, EfRepository<Language, LanguageId>>();
        services.AddScoped<IRepository<Localization, LocalizationId>, EfRepository<Localization, LocalizationId>>();
        services.AddScoped<IRepository<Country, CountryId>, EfRepository<Country, CountryId>>();
        services.AddScoped<IRepository<Region, RegionId>, EfRepository<Region, RegionId>>();
        services.AddScoped<IRepository<TimeZoneReference, TimeZoneReferenceId>, EfRepository<TimeZoneReference, TimeZoneReferenceId>>();
        services.AddScoped<IRepository<Calendar, CalendarId>, EfRepository<Calendar, CalendarId>>();
        services.AddScoped<IRepository<EngineeringUnit, EngineeringUnitId>, EfRepository<EngineeringUnit, EngineeringUnitId>>();
        services.AddScoped<IRepository<DeploymentVersion, DeploymentVersionId>, EfRepository<DeploymentVersion, DeploymentVersionId>>();

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("CorePlatform");

        services.AddScoped<IAppSettingFinder, EfAppSettingFinder>();
        services.AddScoped<IFeatureFlagFinder, EfFeatureFlagFinder>();
        services.AddScoped<IEngineeringUnitFinder, EfEngineeringUnitFinder>();
        services.AddScoped<IDeploymentVersionFinder, EfDeploymentVersionFinder>();
        services.AddScoped<ILanguageFinder, EfLanguageFinder>();
        services.AddScoped<ILocalizationFinder, EfLocalizationFinder>();

        return services;
    }
}
