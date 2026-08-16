using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

/// <summary>UserRole is composite-keyed (plain class, not Entity&lt;TId&gt;, see UserRole's own doc comment) — it needs its own writer, not the generic IRepository&lt;TRoot,TId&gt; shape.</summary>
public interface IUserRoleWriter
{
    Task<bool> ExistsAsync(ApplicationUserId applicationUserId, ApplicationRoleId applicationRoleId, CancellationToken cancellationToken);

    Task AddAsync(UserRole userRole, CancellationToken cancellationToken);
}
