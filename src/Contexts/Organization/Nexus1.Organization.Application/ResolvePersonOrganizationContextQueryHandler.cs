using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

public sealed class ResolvePersonOrganizationContextQueryHandler(IPersonOrganizationContextFinder finder)
    : IQueryHandler<ResolvePersonOrganizationContextQuery, PersonOrganizationContextDto>
{
    public async Task<Result<PersonOrganizationContextDto>> Handle(
        ResolvePersonOrganizationContextQuery query, CancellationToken cancellationToken)
    {
        if (query.PersonId is null && query.ApplicationUserId is null)
        {
            return Result<PersonOrganizationContextDto>.Failure("Either PersonId or ApplicationUserId must be provided.");
        }

        var context = await finder.ResolveAsync(query.PersonId, query.ApplicationUserId, cancellationToken);

        return context is null
            ? Result<PersonOrganizationContextDto>.Failure("Person not found.")
            : Result<PersonOrganizationContextDto>.Success(context);
    }
}
