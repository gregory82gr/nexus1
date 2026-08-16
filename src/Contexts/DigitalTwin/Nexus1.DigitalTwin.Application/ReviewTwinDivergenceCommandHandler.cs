using Nexus1.BuildingBlocks.Application;
using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.Application;

public sealed class ReviewTwinDivergenceCommandHandler(
    IRepository<TwinDivergence, TwinDivergenceId> divergenceRepository,
    IRepository<TwinDivergenceReview, TwinDivergenceReviewId> reviewRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<ReviewTwinDivergenceCommand, long>
{
    public async Task<Result<long>> Handle(ReviewTwinDivergenceCommand command, CancellationToken cancellationToken)
    {
        var divergenceId = new TwinDivergenceId(command.TwinDivergenceId);
        if (await divergenceRepository.GetByIdAsync(divergenceId, cancellationToken) is null)
        {
            return Result<long>.Failure($"TwinDivergence {command.TwinDivergenceId} does not exist.");
        }

        var review = TwinDivergenceReview.Create(
            new TwinDivergenceReviewId(idGenerator.NextLong()), divergenceId, new DivergenceStatusId(command.DivergenceStatusId),
            command.ReviewedAtUtc, command.ReviewedByUserId, command.ReviewNote, command.CorrectiveAction);

        await reviewRepository.AddAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(review.Id.Value);
    }
}
