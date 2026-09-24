using Nexus1.BuildingBlocks.Application;
using Nexus1.Contracts.RootCause;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

/// <summary>
/// Opens a provenance-originated case (ADR-040) and publishes RootCauseCaseOpenedV1 with a
/// null AlarmFloodId through the existing transactional outbox — parallel to
/// OpenAnalysisCommandHandler, but for a case with no flood. Such a case is expected to be
/// terminated with MarkAnalysisInconclusive (the engine has no reach here).
/// </summary>
public sealed class OpenProvenanceAnalysisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    [Microsoft.Extensions.DependencyInjection.FromKeyedServices("RootCause")] IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator,
    IOutboxWriter outboxWriter)
    : ICommandHandler<OpenProvenanceAnalysisCommand, long>
{
    private const string RoutingKey = "root-cause.root-cause-case-opened.v1";
    private const string EventType = "nexus1.root-cause.root-cause-case-opened.v1";

    public async Task<Result<long>> Handle(OpenProvenanceAnalysisCommand command, CancellationToken cancellationToken)
    {
        RootCauseAnalysis analysis;
        try
        {
            analysis = RootCauseAnalysis.OpenForProvenance(
                new RootCauseAnalysisId(idGenerator.NextLong()),
                new UnitId(command.UnitId),
                command.OpenedBy,
                dateTimeProvider.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await repository.AddAsync(analysis, cancellationToken);

        // Same transaction as the aggregate write (ADR-008). AlarmFloodId is null -- a
        // genuinely flood-less case, not a sentinel (ADR-040).
        outboxWriter.Enqueue(
            EventType, schemaVersion: 1, RoutingKey, analysis.OpenedAtUtc,
            new RootCauseCaseOpenedV1(analysis.Id.Value, analysis.UnitId.Value, AlarmFloodId: null, analysis.OpenedAtUtc));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(analysis.Id.Value);
    }
}
