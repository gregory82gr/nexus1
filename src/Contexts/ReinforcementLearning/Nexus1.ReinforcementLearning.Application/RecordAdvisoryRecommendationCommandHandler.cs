using Nexus1.BuildingBlocks.Application;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class RecordAdvisoryRecommendationCommandHandler(
    IRepository<AdvisoryRecommendation, AdvisoryRecommendationId> advisoryRecommendationRepository,
    IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<RecordAdvisoryRecommendationCommand, long>
{
    public async Task<Result<long>> Handle(RecordAdvisoryRecommendationCommand command, CancellationToken cancellationToken)
    {
        AdvisoryRecommendation advisoryRecommendation;
        try
        {
            advisoryRecommendation = AdvisoryRecommendation.Create(
                new AdvisoryRecommendationId(idGenerator.NextLong()), new AdvisorySessionId(command.AdvisorySessionId),
                new RecommendationStatusId(command.RecommendationStatusId), new StateDefinitionId(command.StateDefinitionId),
                new ActionDefinitionId(command.RecommendedActionDefinitionId), command.RequestedAtUtc,
                command.ClampedActionDefinitionId.HasValue ? new ActionDefinitionId(command.ClampedActionDefinitionId.Value) : null,
                command.ObservedPowerPercent, command.TargetPowerPercent, command.ConfidenceScore, command.WasClamped,
                command.ClampReason, command.ExpiresAtUtc);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await advisoryRecommendationRepository.AddAsync(advisoryRecommendation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(advisoryRecommendation.Id.Value);
    }
}
