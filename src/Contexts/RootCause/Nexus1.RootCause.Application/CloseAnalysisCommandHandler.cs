using System.Diagnostics;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Contracts.RootCause;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class CloseAnalysisCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IOutboxWriter outboxWriter)
    : ICommandHandler<CloseAnalysisCommand>
{
    /// <summary>Routing key and eventType per ADR-008's naming convention, frozen coordinates from ch.34 (Executable Asset 34-P).</summary>
    private const string RoutingKey = "root-cause.root-cause-verdict-issued.v1";
    private const string EventType = "nexus1.root-cause.root-cause-verdict-issued.v1";

    public async Task<Result> Handle(CloseAnalysisCommand command, CancellationToken cancellationToken)
    {
        using var activity = NexusActivitySources.RootCauseSource.StartActivity(
            SpanNames.RootCauseVerdictCommit, ActivityKind.Internal, parentContext: default,
            tags: SafeTags.ForOwnerOperation(messageId: null, "ATTEMPTED"));

        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result.Failure($"Root-cause analysis {command.AnalysisId} does not exist.");
        }

        var nowUtc = dateTimeProvider.UtcNow;
        try
        {
            analysis.Close(command.Verdict, command.ClosedBy, nowUtc);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result.Failure(ex.Message);
        }

        try
        {
            // Same transaction as the Closed status commit — outbox row and
            // aggregate write land together in the single SaveChangesAsync below,
            // or neither does (ADR-008/ADR-010: transactional outbox).
            outboxWriter.Enqueue(
                EventType, schemaVersion: 1, RoutingKey, nowUtc,
                new RootCauseVerdictIssuedV1(
                    analysis.Id.Value, analysis.UnitId.Value, analysis.AlarmFloodId.Value, analysis.Verdict!, nowUtc));

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
