using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Messaging;

namespace Nexus1.RootCause.ComponentTests;

public sealed class OpenAnalysisCommandHandlerTests : RootCauseComponentTestDatabase
{
    [Fact]
    public async Task Opening_an_analysis_persists_it_and_is_readable_afterward()
    {
        await using var dbContext = CreateDbContext();
        var handler = new OpenAnalysisCommandHandler(
            Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator(), new EfOutboxWriter(dbContext));

        var result = await handler.Handle(new OpenAnalysisCommand(1, 100, "operator.1"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var analysis = await verifyContext.RootCauseAnalyses.SingleAsync(a => a.Id == new RootCauseAnalysisId(result.Value));
        Assert.Equal(new UnitId(1), analysis.UnitId);
        Assert.Equal(new AlarmFloodId(100), analysis.AlarmFloodId);
        Assert.Equal(AnalysisStatus.Open, analysis.Status);
    }

    [Fact]
    public async Task Opening_an_analysis_with_a_blank_opener_fails_without_writing_anything()
    {
        await using var dbContext = CreateDbContext();
        var handler = new OpenAnalysisCommandHandler(
            Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator(), new EfOutboxWriter(dbContext));

        var result = await handler.Handle(new OpenAnalysisCommand(1, 100, ""), CancellationToken.None);

        Assert.True(result.IsFailure);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.RootCauseAnalyses.CountAsync());
    }
}
