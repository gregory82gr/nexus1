using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.ReactorFleet.Application;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.ReactorFleet.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test
/// (created + migrated in InitializeAsync, dropped in DisposeAsync).
/// </summary>
public sealed class RecordUnitPowerSnapshotCommandHandlerTests : IAsyncLifetime
{
    private readonly string _connectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=ReactorFleetComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

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

    private ReactorFleetDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(_connectionString).Options);

    private RecordUnitPowerSnapshotCommandHandler CreateHandler(ReactorFleetDbContext dbContext) => new(
        new EfRepository<Unit, UnitId>(dbContext),
        new EfRepository<UnitPowerSnapshot, UnitPowerSnapshotId>(dbContext),
        new EfUnitOfWork(dbContext),
        new SystemDateTimeProvider(),
        new SequentialIdGenerator());

    private async Task SeedUnitAsync()
    {
        await using var seedContext = CreateDbContext();
        var unit = Unit.Create(new UnitId(1), "UNIT-1", "Demonstrator Unit 1");
        await seedContext.Units.AddAsync(unit);
        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Recording_a_snapshot_for_an_existing_unit_persists_it_and_is_readable_afterward()
    {
        await SeedUnitAsync();

        // Handler runs against a fresh DbContext, like a real request would get.
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordUnitPowerSnapshotCommand(1, 83.2m), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Read back with an independent DbContext to prove it was actually
        // committed to the database, not just tracked in memory.
        await using var verifyContext = CreateDbContext();
        var snapshot = await verifyContext.UnitPowerSnapshots
            .SingleAsync(s => s.Id == new UnitPowerSnapshotId(result.Value));
        Assert.Equal(new UnitId(1), snapshot.UnitId);
        Assert.Equal(83.2m, snapshot.PowerPercent.Value);
    }

    [Fact]
    public async Task Recording_a_snapshot_for_a_nonexistent_unit_fails_without_writing_anything()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordUnitPowerSnapshotCommand(999, 50m), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("999", result.Error);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.UnitPowerSnapshots.CountAsync());
    }

    [Fact]
    public async Task Recording_an_out_of_range_power_percent_fails_without_writing_anything()
    {
        await SeedUnitAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordUnitPowerSnapshotCommand(1, 250m), CancellationToken.None);

        Assert.True(result.IsFailure);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.UnitPowerSnapshots.CountAsync());
    }
}
