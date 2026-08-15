using Nexus1.BuildingBlocks.Application;
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
        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
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
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
