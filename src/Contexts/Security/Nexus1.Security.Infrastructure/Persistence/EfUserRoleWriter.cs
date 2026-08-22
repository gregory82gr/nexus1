using Microsoft.EntityFrameworkCore;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence;

internal sealed class EfUserRoleWriter(SecurityDbContext dbContext) : IUserRoleWriter
{
    public async Task<bool> ExistsAsync(ApplicationUserId applicationUserId, ApplicationRoleId applicationRoleId, CancellationToken cancellationToken) =>
        await dbContext.UserRoles.AnyAsync(
            x => x.ApplicationUserId == applicationUserId && x.ApplicationRoleId == applicationRoleId, cancellationToken);

    public async Task AddAsync(UserRole userRole, CancellationToken cancellationToken) =>
        await dbContext.UserRoles.AddAsync(userRole, cancellationToken);
}
