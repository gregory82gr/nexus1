using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;
using Nexus1.Organization.Infrastructure.Persistence;

namespace Nexus1.Organization.ComponentTests;

public sealed class GetSitePlantHierarchyQueryHandlerTests : OrganizationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private static GetSitePlantHierarchyQueryHandler CreateHandler(OrganizationDbContext dbContext) =>
        new(new EfSitePlantHierarchyFinder(dbContext));

    private async Task SeedSiteWithPlantsAsync()
    {
        await using var seedContext = CreateDbContext();

        var legalEntityType = LegalEntityType.Create(new LegalEntityTypeId(1), "OPERATOR", "Operator", NowUtc);
        var siteType = SiteType.Create(new SiteTypeId(1), "PLANT_SITE", "Plant Site", NowUtc);
        var plantType = PlantType.Create(new PlantTypeId(1), "NUCLEAR_DEMO", "Nuclear Demo", NowUtc);
        await seedContext.LegalEntityTypes.AddAsync(legalEntityType);
        await seedContext.SiteTypes.AddAsync(siteType);
        await seedContext.PlantTypes.AddAsync(plantType);

        var legalEntity = LegalEntity.Create(new LegalEntityId(1), legalEntityType.Id, "NEXUS1-OP", "Nexus1 Operator", NowUtc);
        await seedContext.LegalEntities.AddAsync(legalEntity);

        var site = Site.Create(new SiteId(1), legalEntity.Id, siteType.Id, countryId: 1, timeZoneId: 1, "SITE-A", "Site A", NowUtc);
        await seedContext.Sites.AddAsync(site);

        await seedContext.Plants.AddAsync(Plant.Create(new PlantId(1), site.Id, plantType.Id, "PLANT-B", "Plant B", NowUtc));
        await seedContext.Plants.AddAsync(Plant.Create(new PlantId(2), site.Id, plantType.Id, "PLANT-A", "Plant A", NowUtc));

        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Returns_the_site_and_its_plants_ordered_by_code()
    {
        await SeedSiteWithPlantsAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetSitePlantHierarchyQuery("SITE-A"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Site A", result.Value.SiteName);
        Assert.Equal(2, result.Value.Plants.Count);
        Assert.Equal("PLANT-A", result.Value.Plants[0].Code);
        Assert.Equal("PLANT-B", result.Value.Plants[1].Code);
    }

    [Fact]
    public async Task Nonexistent_site_code_fails()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new GetSitePlantHierarchyQuery("NO-SUCH-SITE"), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
