using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.ComponentTests;

public sealed class DefineAlarmCommandHandlerTests : AlarmManagementComponentTestDatabase
{
    [Fact]
    public async Task Defining_an_alarm_persists_it_and_is_readable_afterward()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DefineAlarmCommandHandler(
            Repository<AlarmDefinition, AlarmDefinitionId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

        var result = await handler.Handle(
            new DefineAlarmCommand(1, "HIGH-POWER", "High Power", AlarmSeverity.High, 100m), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var definition = await verifyContext.AlarmDefinitions.SingleAsync(d => d.Id == new AlarmDefinitionId(result.Value));
        Assert.Equal(new UnitId(1), definition.UnitId);
        Assert.Equal("HIGH-POWER", definition.Code);
        Assert.Equal(100m, definition.ThresholdValue);
    }

    [Fact]
    public async Task Defining_an_alarm_with_a_blank_code_fails_without_writing_anything()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DefineAlarmCommandHandler(
            Repository<AlarmDefinition, AlarmDefinitionId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

        var result = await handler.Handle(
            new DefineAlarmCommand(1, "", "High Power", AlarmSeverity.High, 100m), CancellationToken.None);

        Assert.True(result.IsFailure);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.AlarmDefinitions.CountAsync());
    }
}
