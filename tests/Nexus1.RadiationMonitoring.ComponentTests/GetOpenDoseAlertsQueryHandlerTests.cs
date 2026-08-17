using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

/// <summary>
/// Matches the atlas's own C.13.5.2 query 4, adapted: open dose alerts and
/// the person assignment that produced them. Asserts PersonId (not a
/// display name) — see OpenDoseAlertDto's own doc comment for why this
/// deviates from the atlas's literal query text (Organization.Person is
/// passport-only in this codebase, ADR-024).
/// </summary>
public sealed class GetOpenDoseAlertsQueryHandlerTests : RadiationMonitoringComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_open_and_acknowledged_alerts_with_the_producing_reading()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var alertSeedContext = CreateDbContext())
        {
            var doseReading = PersonDoseReading.Create(
                new PersonDoseReadingId(1), new PersonDosimeterAssignmentId(seed.PersonDosimeterAssignmentId),
                new DoseTypeId(seed.DoseTypeId), seed.EngineeringUnitId, new MeasurementQualityId(seed.MeasurementQualityId),
                NowUtc, 21m, isFinal: true);
            await alertSeedContext.PersonDoseReadings.AddAsync(doseReading);
            await alertSeedContext.SaveChangesAsync();

            var openAlert = DoseAlert.Create(
                new DoseAlertId(1), new DoseLimitId(seed.DoseLimitId), new AlertStatusId(seed.AlertStatusOpenId),
                NowUtc, "Annual dose limit exceeded", personDoseReadingId: doseReading.Id);
            var closedAlert = DoseAlert.Create(
                new DoseAlertId(2), new DoseLimitId(seed.DoseLimitId), new AlertStatusId(seed.AlertStatusClosedId),
                NowUtc.AddDays(-10), "Resolved dose alert", personDoseReadingId: doseReading.Id);

            await alertSeedContext.DoseAlerts.AddAsync(openAlert);
            await alertSeedContext.DoseAlerts.AddAsync(closedAlert);
            await alertSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetOpenDoseAlertsQueryHandler(new EfOpenDoseAlertsFinder(dbContext));

        var result = await handler.Handle(new GetOpenDoseAlertsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var alert = Assert.Single(result.Value);
        Assert.Equal("Annual dose limit exceeded", alert.Message);
        Assert.Equal(RadiationMonitoringSeedHelper.DoseLimitCode, alert.LimitCode);
        Assert.Equal(seed.PersonId, alert.PersonId);
        Assert.Equal(21m, alert.DoseValue);
        Assert.Equal(RadiationMonitoringSeedHelper.EngineeringUnitSymbol, alert.EngineeringUnitSymbol);
    }
}
