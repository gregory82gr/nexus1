using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;
using Nexus1.Organization.Infrastructure.Persistence;

namespace Nexus1.Organization.ComponentTests;

public sealed class RecordStaffingScenarioResultCommandHandlerTests : OrganizationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private static RecordStaffingScenarioResultCommandHandler CreateHandler(OrganizationDbContext dbContext) => new(
        new EfRepository<StaffingScenario, StaffingScenarioId>(dbContext),
        new EfRepository<StaffingScenarioResult, StaffingScenarioResultId>(dbContext),
        new EfRepository<StaffingScenarioGap, StaffingScenarioGapId>(dbContext),
        UnitOfWork(dbContext),
        new SequentialIdGenerator());

    private async Task SeedScenarioAndPositionAsync()
    {
        await using var seedContext = CreateDbContext();

        var legalEntityType = LegalEntityType.Create(new LegalEntityTypeId(1), "OPERATOR", "Operator", NowUtc);
        var siteType = SiteType.Create(new SiteTypeId(1), "PLANT_SITE", "Plant Site", NowUtc);
        await seedContext.LegalEntityTypes.AddAsync(legalEntityType);
        await seedContext.SiteTypes.AddAsync(siteType);

        var legalEntity = LegalEntity.Create(new LegalEntityId(1), legalEntityType.Id, "NEXUS1-OP", "Nexus1 Operator", NowUtc);
        await seedContext.LegalEntities.AddAsync(legalEntity);

        var site = Site.Create(new SiteId(1), legalEntity.Id, siteType.Id, countryId: 1, timeZoneId: 1, "SITE-A", "Site A", NowUtc);
        await seedContext.Sites.AddAsync(site);

        await seedContext.StaffingScenarios.AddAsync(
            StaffingScenario.Create(new StaffingScenarioId(1), site.Id, "OUTAGE-1", "Outage Scenario 1", NowUtc));

        await seedContext.Positions.AddAsync(
            Position.Create(new PositionId(1), "REACTOR-OP", "Reactor Operator", NowUtc));

        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Recording_a_result_with_gaps_persists_result_and_gaps_with_computed_gap_count()
    {
        await SeedScenarioAndPositionAsync();

        await using var dbContext = CreateDbContext();
        var command = new RecordStaffingScenarioResultCommand(
            1, NowUtc, "Fail", [new StaffingGapRequest(1, RequiredCount: 5, AvailableCount: 3)]);
        var result = await CreateHandler(dbContext).Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var storedResult = await verifyContext.StaffingScenarioResults.SingleAsync();
        Assert.Equal("Fail", storedResult.OverallStatus);

        var gap = await verifyContext.StaffingScenarioGaps.SingleAsync();
        Assert.Equal(5, gap.RequiredCount);
        Assert.Equal(3, gap.AvailableCount);
        Assert.Equal(2, gap.GapCount); // computed by the database's own computed column, round-tripped on read
    }

    [Fact]
    public async Task Recording_a_result_for_a_nonexistent_scenario_fails_without_writing_anything()
    {
        await SeedScenarioAndPositionAsync();

        await using var dbContext = CreateDbContext();
        var command = new RecordStaffingScenarioResultCommand(999, NowUtc, "Pass", []);
        var result = await CreateHandler(dbContext).Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.StaffingScenarioResults.CountAsync());
    }

    [Fact]
    public async Task Recording_with_an_invalid_overall_status_fails_without_writing_anything()
    {
        await SeedScenarioAndPositionAsync();

        await using var dbContext = CreateDbContext();
        var command = new RecordStaffingScenarioResultCommand(1, NowUtc, "Unknown", []);
        var result = await CreateHandler(dbContext).Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.StaffingScenarioResults.CountAsync());
    }
}
