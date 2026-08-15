using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Messaging;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// Exercises Open -> AddHypothesis -> AddEvidence -> RejectHypothesis -> Close
/// across independent DbContext instances end to end, proving
/// RootCauseAnalysisRepository's explicit Include(...).ThenInclude(...) really
/// reloads the full aggregate graph on every handler call, not just within a
/// single in-memory DbContext lifetime.
/// </summary>
public sealed class FullAnalysisWorkflowTests : RootCauseComponentTestDatabase
{
    private async Task<long> OpenAnalysisAsync()
    {
        await using var dbContext = CreateDbContext();
        var result = await new OpenAnalysisCommandHandler(
                Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new SequentialIdGenerator(), new EfOutboxWriter(dbContext))
            .Handle(new OpenAnalysisCommand(1, 100, "operator.1"), CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private async Task<int> AddHypothesisAsync(long analysisId, string statement)
    {
        await using var dbContext = CreateDbContext();
        var result = await new AddHypothesisCommandHandler(Repository(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator())
            .Handle(new AddHypothesisCommand(analysisId, statement), CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    [Fact]
    public async Task Closing_without_any_evidence_fails()
    {
        var analysisId = await OpenAnalysisAsync();
        await AddHypothesisAsync(analysisId, "Loose fitting on primary loop.");

        await using var dbContext = CreateDbContext();
        var result = await new CloseAnalysisCommandHandler(
                Repository(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider(), new EfOutboxWriter(dbContext))
            .Handle(new CloseAnalysisCommand(analysisId, "Confirmed", "operator.1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("A root-cause case cannot close without evidence.", result.Error);

        await using var verifyContext = CreateDbContext();
        var analysis = await verifyContext.RootCauseAnalyses.SingleAsync(a => a.Id == new RootCauseAnalysisId(analysisId));
        Assert.Equal(AnalysisStatus.Open, analysis.Status);
    }

    [Fact]
    public async Task Closing_with_every_hypothesis_rejected_fails()
    {
        var analysisId = await OpenAnalysisAsync();
        var hypothesisId = await AddHypothesisAsync(analysisId, "Loose fitting on primary loop.");

        await using (var evidenceContext = CreateDbContext())
        {
            var result = await new AddEvidenceCommandHandler(
                    Repository(evidenceContext), UnitOfWork(evidenceContext), new SystemDateTimeProvider(), new SequentialIdGenerator())
                .Handle(new AddEvidenceCommand(analysisId, hypothesisId, "Inspection photo."), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using (var rejectContext = CreateDbContext())
        {
            var result = await new RejectHypothesisCommandHandler(Repository(rejectContext), UnitOfWork(rejectContext), new SystemDateTimeProvider())
                .Handle(new RejectHypothesisCommand(analysisId, hypothesisId, "Ruled out by inspection."), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using var closeContext = CreateDbContext();
        var closeResult = await new CloseAnalysisCommandHandler(
                Repository(closeContext), UnitOfWork(closeContext), new SystemDateTimeProvider(), new EfOutboxWriter(closeContext))
            .Handle(new CloseAnalysisCommand(analysisId, "Confirmed", "operator.1"), CancellationToken.None);

        Assert.True(closeResult.IsFailure);
        Assert.Equal("At least one hypothesis must remain supported or accepted.", closeResult.Error);
    }

    [Fact]
    public async Task Full_workflow_with_evidence_and_a_surviving_hypothesis_closes_successfully()
    {
        var analysisId = await OpenAnalysisAsync();
        var survivingHypothesisId = await AddHypothesisAsync(analysisId, "Loose fitting on primary loop.");
        var rejectedHypothesisId = await AddHypothesisAsync(analysisId, "Sensor drift.");

        await using (var evidenceContext = CreateDbContext())
        {
            var result = await new AddEvidenceCommandHandler(
                    Repository(evidenceContext), UnitOfWork(evidenceContext), new SystemDateTimeProvider(), new SequentialIdGenerator())
                .Handle(new AddEvidenceCommand(analysisId, survivingHypothesisId, "Inspection photo."), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using (var rejectContext = CreateDbContext())
        {
            var result = await new RejectHypothesisCommandHandler(Repository(rejectContext), UnitOfWork(rejectContext), new SystemDateTimeProvider())
                .Handle(new RejectHypothesisCommand(analysisId, rejectedHypothesisId, "Calibration log clean."), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using (var closeContext = CreateDbContext())
        {
            var result = await new CloseAnalysisCommandHandler(
                    Repository(closeContext), UnitOfWork(closeContext), new SystemDateTimeProvider(), new EfOutboxWriter(closeContext))
                .Handle(new CloseAnalysisCommand(analysisId, "Loose fitting confirmed as cause.", "operator.2"), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        // Read the final state back through the query handler, independently.
        await using var queryContext = CreateDbContext();
        var queryResult = await new GetAnalysisByIdQueryHandler(Repository(queryContext))
            .Handle(new GetAnalysisByIdQuery(analysisId), CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var dto = queryResult.Value;
        Assert.NotNull(dto);
        Assert.Equal("Closed", dto!.Status);
        Assert.Equal("Loose fitting confirmed as cause.", dto.Verdict);
        Assert.Equal(2, dto.Hypotheses.Count);

        var survivingDto = dto.Hypotheses.Single(h => h.HypothesisId == survivingHypothesisId);
        Assert.Equal("Proposed", survivingDto.Status);
        Assert.Equal(1, survivingDto.EvidenceCount);

        var rejectedDto = dto.Hypotheses.Single(h => h.HypothesisId == rejectedHypothesisId);
        Assert.Equal("Rejected", rejectedDto.Status);
        Assert.Equal(0, rejectedDto.EvidenceCount);
    }

    [Fact]
    public async Task Mutating_a_closed_analysis_fails()
    {
        var analysisId = await OpenAnalysisAsync();
        var hypothesisId = await AddHypothesisAsync(analysisId, "Loose fitting on primary loop.");

        await using (var evidenceContext = CreateDbContext())
        {
            await new AddEvidenceCommandHandler(
                    Repository(evidenceContext), UnitOfWork(evidenceContext), new SystemDateTimeProvider(), new SequentialIdGenerator())
                .Handle(new AddEvidenceCommand(analysisId, hypothesisId, "Inspection photo."), CancellationToken.None);
        }

        await using (var closeContext = CreateDbContext())
        {
            var result = await new CloseAnalysisCommandHandler(
                    Repository(closeContext), UnitOfWork(closeContext), new SystemDateTimeProvider(), new EfOutboxWriter(closeContext))
                .Handle(new CloseAnalysisCommand(analysisId, "Confirmed", "operator.1"), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using var dbContext = CreateDbContext();
        var addHypothesisResult = await new AddHypothesisCommandHandler(Repository(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator())
            .Handle(new AddHypothesisCommand(analysisId, "Another cause."), CancellationToken.None);

        Assert.True(addHypothesisResult.IsFailure);
        Assert.Equal("Closed cases cannot be changed.", addHypothesisResult.Error);
    }
}
