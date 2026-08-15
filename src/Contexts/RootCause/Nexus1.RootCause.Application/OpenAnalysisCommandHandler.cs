using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class OpenAnalysisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator)
    : ICommandHandler<OpenAnalysisCommand, long>
{
    public async Task<Result<long>> Handle(OpenAnalysisCommand command, CancellationToken cancellationToken)
    {
        RootCauseAnalysis analysis;
        try
        {
            analysis = RootCauseAnalysis.Open(
                new RootCauseAnalysisId(idGenerator.NextLong()),
                new UnitId(command.UnitId),
                new AlarmFloodId(command.AlarmFloodId),
                command.OpenedBy,
                dateTimeProvider.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await repository.AddAsync(analysis, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(analysis.Id.Value);
    }
}
