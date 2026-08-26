using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Application;

public sealed class ExtractPolicyCommandHandler(
    IRepository<Policy, PolicyId> policyRepository, [FromKeyedServices("ReinforcementLearning")] IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<ExtractPolicyCommand, int>
{
    public async Task<Result<int>> Handle(ExtractPolicyCommand command, CancellationToken cancellationToken)
    {
        Policy policy;
        try
        {
            policy = Policy.Create(
                new PolicyId(idGenerator.NextInt()), new QTableId(command.QTableId),
                new PolicyStatusId(command.PolicyStatusId), command.Code, command.Name, command.ExtractedAtUtc,
                command.EntryCount);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await policyRepository.AddAsync(policy, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(policy.Id.Value);
    }
}
