namespace Nexus1.Security.Domain;

/// <summary>
/// Many-to-many assignment of users to roles, with expiry and assignment
/// audit (atlas C.2.4.11). Composite-keyed (ApplicationUserId +
/// ApplicationRoleId), so — like InboxReceipt/RetryTicket elsewhere in this
/// codebase — it is a plain class, not an Entity&lt;TId&gt; aggregate: there is
/// no single surrogate identity to hang IAggregateRoot's generic
/// IRepository&lt;TRoot,TId&gt; shape on.
/// </summary>
public sealed class UserRole
{
    public UserRole(
        ApplicationUserId applicationUserId, ApplicationRoleId applicationRoleId, DateTime assignedAtUtc,
        ApplicationUserId? assignedByUserId = null, DateTime? expiresAtUtc = null, bool isActive = true)
    {
        if (expiresAtUtc is not null && expiresAtUtc <= assignedAtUtc)
        {
            throw new ArgumentException("ExpiresAtUtc must be later than AssignedAtUtc when present.", nameof(expiresAtUtc));
        }

        ApplicationUserId = applicationUserId;
        ApplicationRoleId = applicationRoleId;
        AssignedAtUtc = assignedAtUtc;
        AssignedByUserId = assignedByUserId;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = isActive;
    }

    public ApplicationUserId ApplicationUserId { get; }

    public ApplicationRoleId ApplicationRoleId { get; }

    public DateTime AssignedAtUtc { get; }

    public ApplicationUserId? AssignedByUserId { get; }

    public DateTime? ExpiresAtUtc { get; }

    public bool IsActive { get; private set; }

    public bool IsActiveAt(DateTime nowUtc) => IsActive && (ExpiresAtUtc is null || ExpiresAtUtc > nowUtc);

    public void Revoke() => IsActive = false;
}
