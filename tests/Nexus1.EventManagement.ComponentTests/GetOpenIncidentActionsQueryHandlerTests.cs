using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

/// <summary>Matches the atlas's own C.8.5.2 query 3, verbatim: incident actions where status NOT IN (COMPLETED, VERIFIED, CANCELLED), with incident number, ordered by DueAtUtc.</summary>
public sealed class GetOpenIncidentActionsQueryHandlerTests : EventManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Dedicated proof (ADR-022) that COMPLETED/VERIFIED/CANCELLED actions are excluded, not merely that an open one is returned.</summary>
    [Fact]
    public async Task Excludes_completed_verified_and_cancelled_actions_and_returns_only_open_ones_ordered_by_due_date()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        await using (var actionSeedContext = CreateDbContext())
        {
            var operationalEvent = OperationalEvent.Create(
                new OperationalEventId(1), seed.UnitId, new EventTypeId(seed.EventTypeId), new EventStatusId(seed.EventStatusId),
                new EventSeverityId(seed.EventSeverityId), new EventSourceTypeId(seed.EventSourceTypeId),
                EventManagementSeedHelper.EventCode, "Feedwater flow deviation", NowUtc, NowUtc.AddMinutes(5));
            await actionSeedContext.OperationalEvents.AddAsync(operationalEvent);
            await actionSeedContext.SaveChangesAsync();

            var incident = Incident.Create(
                new IncidentId(1), operationalEvent.Id.Value, new IncidentTypeId(seed.IncidentTypeId),
                new IncidentStatusId(seed.IncidentStatusId), "INC-2026-0007", NowUtc);
            await actionSeedContext.Incidents.AddAsync(incident);
            await actionSeedContext.SaveChangesAsync();

            var soonestOpenAction = IncidentAction.Create(
                new IncidentActionId(1), incident.Id.Value, new IncidentActionTypeId(seed.IncidentActionTypeId),
                new IncidentActionStatusId(seed.IncidentActionStatusOpenId), "Replace corroded valve", dueAtUtc: NowUtc.AddDays(3));
            var laterOpenAction = IncidentAction.Create(
                new IncidentActionId(2), incident.Id.Value, new IncidentActionTypeId(seed.IncidentActionTypeId),
                new IncidentActionStatusId(seed.IncidentActionStatusOpenId), "Inspect adjacent piping", dueAtUtc: NowUtc.AddDays(10));
            var completedAction = IncidentAction.Create(
                new IncidentActionId(3), incident.Id.Value, new IncidentActionTypeId(seed.IncidentActionTypeId),
                new IncidentActionStatusId(seed.IncidentActionStatusCompletedId), "Already completed", dueAtUtc: NowUtc.AddDays(1));
            var verifiedAction = IncidentAction.Create(
                new IncidentActionId(4), incident.Id.Value, new IncidentActionTypeId(seed.IncidentActionTypeId),
                new IncidentActionStatusId(seed.IncidentActionStatusVerifiedId), "Already verified", dueAtUtc: NowUtc.AddDays(1));
            var cancelledAction = IncidentAction.Create(
                new IncidentActionId(5), incident.Id.Value, new IncidentActionTypeId(seed.IncidentActionTypeId),
                new IncidentActionStatusId(seed.IncidentActionStatusCancelledId), "Cancelled action", dueAtUtc: NowUtc.AddDays(1));

            await actionSeedContext.IncidentActions.AddRangeAsync(
                laterOpenAction, soonestOpenAction, completedAction, verifiedAction, cancelledAction);
            await actionSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetOpenIncidentActionsQueryHandler(new EfOpenIncidentActionsFinder(dbContext));

        var result = await handler.Handle(new GetOpenIncidentActionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Replace corroded valve", result.Value[0].Title);
        Assert.Equal("Inspect adjacent piping", result.Value[1].Title);
        Assert.All(result.Value, a => Assert.Equal("INC-2026-0007", a.IncidentNumber));
        Assert.DoesNotContain(result.Value, a => a.Title == "Already completed");
        Assert.DoesNotContain(result.Value, a => a.Title == "Already verified");
        Assert.DoesNotContain(result.Value, a => a.Title == "Cancelled action");
    }
}
