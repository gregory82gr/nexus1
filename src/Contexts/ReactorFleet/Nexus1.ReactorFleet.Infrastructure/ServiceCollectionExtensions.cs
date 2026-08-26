using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.ReactorFleet.Application;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.ReactorFleet.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// connectionString points at AlarmManagementDb (ADR-006 — ReactorFleet
    /// shares that physical database, own schema, own migration history).
    /// </summary>
    public static IServiceCollection AddReactorFleetInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ReactorFleetDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_ReactorFleet")));

        services.AddScoped<IRepository<Unit, UnitId>, EfRepository<Unit, UnitId>>();
        services.AddScoped<IRepository<UnitPowerSnapshot, UnitPowerSnapshotId>, EfRepository<UnitPowerSnapshot, UnitPowerSnapshotId>>();
        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("ReactorFleet");

        services.AddScoped<IUnitFleetFinder, EfUnitFleetFinder>();

        return services;
    }
}
