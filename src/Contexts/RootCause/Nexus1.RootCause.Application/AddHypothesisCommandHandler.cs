using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class AddHypothesisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<AddHypothesisCommand, int>
{
    public async Task<Result<int>> Handle(AddHypothesisCommand command, CancellationToken cancellationToken)
    {
        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            return Result<int>.Failure($"Root-cause analysis {command.AnalysisId} does not exist.");
        }

        var hypothesisId = new AnalysisHypothesisId(idGenerator.NextInt());
        try
        {
            analysis.AddHypothesis(hypothesisId, command.HypothesisStatement);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result<int>.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(hypothesisId.Value);
    }
}
