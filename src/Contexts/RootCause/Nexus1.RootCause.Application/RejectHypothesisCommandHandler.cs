using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class RejectHypothesisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RejectHypothesisCommand>
{
    public async Task<Result> Handle(RejectHypothesisCommand command, CancellationToken cancellationToken)
    {
        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            return Result.Failure($"Root-cause analysis {command.AnalysisId} does not exist.");
        }

        try
        {
            analysis.RejectHypothesis(new AnalysisHypothesisId(command.HypothesisId), command.Reason, dateTimeProvider.UtcNow);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
