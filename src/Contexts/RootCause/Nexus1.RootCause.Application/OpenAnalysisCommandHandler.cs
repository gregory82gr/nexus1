using System.Diagnostics;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
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
        using var activity = NexusActivitySources.RootCauseSource.StartActivity(
            SpanNames.RootCauseCaseOpen, ActivityKind.Internal, parentContext: default,
            tags: SafeTags.ForOwnerOperation(messageId: null, "ATTEMPTED"));

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
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result<long>.Failure(ex.Message);
        }

        try
        {
            await repository.AddAsync(analysis, cancellationToken);

            // Same transaction as the analysis' own commit — outbox row and
            // aggregate write land together (ADR-008/ADR-012).
            outboxWriter.Enqueue(
                EventType, schemaVersion: 1, RoutingKey, analysis.OpenedAtUtc,
                new RootCauseCaseOpenedV1(analysis.Id.Value, analysis.UnitId.Value, analysis.AlarmFloodId.Value, analysis.OpenedAtUtc));

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            SafeError.Record(activity, ex);
            throw;
        }

        activity?.SetTag("nexus1.outcome.code", "COMMITTED");
        return Result<long>.Success(analysis.Id.Value);
    }
}
