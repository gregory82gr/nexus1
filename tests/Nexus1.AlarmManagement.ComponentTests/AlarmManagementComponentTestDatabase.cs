using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.AlarmManagement.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test.</summary>
public abstract class AlarmManagementComponentTestDatabase : IAsyncLifetime
{
    private readonly string _connectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=AlarmManagementComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected AlarmManagementDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AlarmManagementDbContext>().UseSqlServer(_connectionString).Options);

    protected static IAlarmDefinitionFinder DefinitionFinder(AlarmManagementDbContext dbContext) => new EfAlarmDefinitionFinder(dbContext);

    protected static IAlarmEventFinder EventFinder(AlarmManagementDbContext dbContext) => new EfAlarmEventFinder(dbContext);

    protected static IUnitOfWork UnitOfWork(AlarmManagementDbContext dbContext) => new EfUnitOfWork(dbContext);

    protected static IRepository<TRoot, TId> Repository<TRoot, TId>(AlarmManagementDbContext dbContext)
        where TRoot : Entity<TId>, IAggregateRoot
        where TId : notnull =>
        new EfRepository<TRoot, TId>(dbContext);
}
