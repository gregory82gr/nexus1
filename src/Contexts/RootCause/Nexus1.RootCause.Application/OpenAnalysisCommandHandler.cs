using Nexus1.BuildingBlocks.Application;
using Nexus1.Contracts.RootCause;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class OpenAnalysisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator,
    IOutboxWriter outboxWriter)
    : ICommandHandler<OpenAnalysisCommand, long>
{
    /// <summary>Routing key/eventType per ADR-008's naming convention, matching RootCauseVerdictIssuedV1's own coordinates (ADR-012).</summary>
    private const string RoutingKey = "root-cause.root-cause-case-opened.v1";
    private const string EventType = "nexus1.root-cause.root-cause-case-opened.v1";

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

        // Same transaction as the analysis' own commit — outbox row and
        // aggregate write land together (ADR-008/ADR-012).
        outboxWriter.Enqueue(
            EventType, schemaVersion: 1, RoutingKey, analysis.OpenedAtUtc,
            new RootCauseCaseOpenedV1(analysis.Id.Value, analysis.UnitId.Value, analysis.AlarmFloodId.Value, analysis.OpenedAtUtc));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(analysis.Id.Value);
    }
}
