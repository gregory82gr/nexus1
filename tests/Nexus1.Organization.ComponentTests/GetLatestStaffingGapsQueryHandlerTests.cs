using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;
using Nexus1.Organization.Infrastructure.Persistence;

namespace Nexus1.Organization.ComponentTests;

/// <summary>
/// Proves GetLatestStaffingGapsQuery picks the most recent
/// StaffingScenarioResult by EvaluatedAtUtc when multiple results exist for
/// the same scenario — mirrors the atlas's own C.3.8 query 3
/// correlated-subquery shape (ADR-017's evidence requirement).
/// </summary>
public sealed class GetLatestStaffingGapsQueryHandlerTests : OrganizationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private static GetLatestStaffingGapsQueryHandler CreateHandler(OrganizationDbContext dbContext) =>
        new(new EfStaffingGapFinder(dbContext));

    private async Task SeedScenarioWithTwoResultsAsync()
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

        var scenario = StaffingScenario.Create(new StaffingScenarioId(1), site.Id, "OUTAGE-1", "Outage Scenario 1", NowUtc);
        await seedContext.StaffingScenarios.AddAsync(scenario);

        var position = Position.Create(new PositionId(1), "REACTOR-OP", "Reactor Operator", NowUtc);
        await seedContext.Positions.AddAsync(position);

        // Older result: evaluated first, would show a gap of 3 if (wrongly) picked.
        var olderResult = StaffingScenarioResult.Create(new StaffingScenarioResultId(1), scenario.Id, NowUtc.AddDays(-1), "Fail");
        await seedContext.StaffingScenarioResults.AddAsync(olderResult);
        await seedContext.StaffingScenarioGaps.AddAsync(
            StaffingScenarioGap.Create(new StaffingScenarioGapId(1), olderResult.Id, position.Id, requiredCount: 5, availableCount: 2));

        // Newer result: evaluated later, is the one GetLatestStaffingGapsQuery must return.
        var newerResult = StaffingScenarioResult.Create(new StaffingScenarioResultId(2), scenario.Id, NowUtc, "Warning");
        await seedContext.StaffingScenarioResults.AddAsync(newerResult);
        await seedContext.StaffingScenarioGaps.AddAsync(
            StaffingScenarioGap.Create(new StaffingScenarioGapId(2), newerResult.Id, position.Id, requiredCount: 5, availableCount: 4));

        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Returns_gaps_from_the_most_recently_evaluated_result_only()
    {
        await SeedScenarioWithTwoResultsAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetLatestStaffingGapsQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var gap = Assert.Single(result.Value);
        Assert.Equal(4, gap.AvailableCount); // from the newer result, not the older one's AvailableCount of 2
        Assert.Equal(1, gap.GapCount);
    }

    [Fact]
    public async Task Scenario_with_no_results_returns_an_empty_list()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetLatestStaffingGapsQuery(999), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
