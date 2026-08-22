using System.Diagnostics;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class AddEvidenceCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator)
    : ICommandHandler<AddEvidenceCommand>
{
    public async Task<Result> Handle(AddEvidenceCommand command, CancellationToken cancellationToken)
    {
        using var activity = NexusActivitySources.RootCauseSource.StartActivity(
            SpanNames.AddEvidence, ActivityKind.Internal, parentContext: default,
            tags: SafeTags.ForOwnerOperation(messageId: null, "ATTEMPTED"));

        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result.Failure($"Root-cause analysis {command.AnalysisId} does not exist.");
        }

        try
        {
            analysis.AddEvidence(
                new AnalysisHypothesisId(command.HypothesisId),
                new HypothesisEvidenceId(idGenerator.NextInt()),
                command.Description,
                dateTimeProvider.UtcNow);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result.Failure(ex.Message);
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
        return Result.Success();
    }
}
