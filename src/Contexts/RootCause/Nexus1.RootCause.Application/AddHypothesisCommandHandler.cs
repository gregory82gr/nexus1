using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class AddHypothesisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    [FromKeyedServices("RootCause")] IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<AddHypothesisCommand, int>
{
    public async Task<Result<int>> Handle(AddHypothesisCommand command, CancellationToken cancellationToken)
    {
        using var activity = NexusActivitySources.RootCauseSource.StartActivity(
            SpanNames.AddHypothesis, ActivityKind.Internal, parentContext: default,
            tags: SafeTags.ForOwnerOperation(messageId: null, "ATTEMPTED"));

        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result<int>.Failure($"Root-cause analysis {command.AnalysisId} does not exist.");
        }

        var hypothesisId = new AnalysisHypothesisId(idGenerator.NextInt());
        try
        {
            analysis.AddHypothesis(hypothesisId, command.HypothesisStatement);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result<int>.Failure(ex.Message);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            SafeError.Record(activity, ex);
            throw;
        }

        activity?.SetTag("nexus1.outcome.code", "COMMITTED");
        return Result<int>.Success(hypothesisId.Value);
    }
}
