using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Contracts.RootCause;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

/// <summary>
/// Marks a case Inconclusive and publishes RootCauseCaseInconclusiveV1 through the
/// existing transactional outbox (ADR-039) — parallel to CloseAnalysisCommandHandler,
/// but it records a reason, never a verdict. An unknown/finalized case is a first-class
/// failure (Result.Failure -> the edge maps it).
///
/// Deferred niceties (named, not silently dropped): no alarm-to-inconclusive
/// workflow-duration metric and no dedicated tracing span this slice — both would touch
/// the reviewed observability vocabulary/catalogue, out of scope for this state-machine
/// slice (ADR-039).
/// </summary>
public sealed class MarkAnalysisInconclusiveCommandHandler(
    IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository,
    [FromKeyedServices("RootCause")] IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IOutboxWriter outboxWriter)
    : ICommandHandler<MarkAnalysisInconclusiveCommand>
{
    /// <summary>Routing key/eventType per ADR-008's naming convention, matching the other RootCause events' coordinates (ADR-039).</summary>
    private const string RoutingKey = "root-cause.root-cause-case-inconclusive.v1";
    private const string EventType = "nexus1.root-cause.root-cause-case-inconclusive.v1";

    public async Task<Result> Handle(MarkAnalysisInconclusiveCommand command, CancellationToken cancellationToken)
    {
        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(command.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            return Result.Failure($"Root-cause analysis {command.AnalysisId} does not exist.");
        }

        var nowUtc = dateTimeProvider.UtcNow;
        try
        {
            analysis.MarkInconclusive(command.Reason, command.DecidedBy, nowUtc);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }

        // Same transaction as the Inconclusive status commit — outbox row and aggregate
        // write land together, or neither does (ADR-008/ADR-010: transactional outbox).
        outboxWriter.Enqueue(
            EventType, schemaVersion: 1, RoutingKey, nowUtc,
            new RootCauseCaseInconclusiveV1(
                analysis.Id.Value, analysis.UnitId.Value, analysis.AlarmFloodId.Value, analysis.InconclusiveReason!, nowUtc));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
