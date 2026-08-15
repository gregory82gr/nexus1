using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class CloseAnalysisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CloseAnalysisCommand>
{
    public async Task<Result> Handle(CloseAnalysisCommand command, CancellationToken cancellationToken)
    {
        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            return Result.Failure($"Root-cause analysis {command.AnalysisId} does not exist.");
        }

        try
        {
            analysis.Close(command.Verdict, command.ClosedBy, dateTimeProvider.UtcNow);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
