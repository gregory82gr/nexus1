using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Security.Domain;

/// <summary>
/// Login-capable account, compatible in shape with ASP.NET Core Identity
/// using integer keys (atlas C.2.4.3/C.2.1 "Design choice"). This project
/// does not implement credential hashing or verification (no HTTP
/// authentication surface exists this phase, ADR-016) — PasswordHash is
/// modeled as an opaque, caller-supplied value, never computed here.
/// IsServiceAccount is the atlas-provided distinction between a human login
/// and an automation identity — the flagged (not yet wired) hook for
/// Phase 1's ad-hoc "system:..." string literals (ADR-016).
/// </summary>
public sealed class ApplicationUser : Entity<ApplicationUserId>, IAggregateRoot
{
    private ApplicationUser(
        ApplicationUserId id, UserStatusId userStatusId, string userName, string normalizedUserName,
        string displayName, bool isServiceAccount, string? email, string? normalizedEmail, string? passwordHash,
        string? givenName, string? familyName, DateTime createdAtUtc)
        : base(id)
    {
        UserStatusId = userStatusId;
        UserName = userName;
        NormalizedUserName = normalizedUserName;
        DisplayName = displayName;
        IsServiceAccount = isServiceAccount;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        GivenName = givenName;
        FamilyName = familyName;
        LockoutEnabled = true;
        AccessFailedCount = 0;
        CreatedAtUtc = createdAtUtc;
    }

    public UserStatusId UserStatusId { get; }

    public string UserName { get; }

    public string NormalizedUserName { get; }

    public string DisplayName { get; }

    public bool IsServiceAccount { get; }

    public string? Email { get; }

    public string? NormalizedEmail { get; }

    public bool EmailConfirmed { get; private set; }

    public string? PasswordHash { get; private set; }

    public string? GivenName { get; }

    public string? FamilyName { get; }

    public bool LockoutEnabled { get; }

    public DateTime? LockoutEndUtc { get; private set; }

    public int AccessFailedCount { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public bool IsLockedOut(DateTime nowUtc) => LockoutEnabled && LockoutEndUtc is { } end && end > nowUtc;

    public static ApplicationUser Create(
        ApplicationUserId id, UserStatusId userStatusId, string userName, string normalizedUserName,
        string displayName, DateTime createdAtUtc, bool isServiceAccount = false, string? email = null,
        string? normalizedEmail = null, string? passwordHash = null, string? givenName = null, string? familyName = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("UserName must not be empty.", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            throw new ArgumentException("NormalizedUserName must not be empty.", nameof(normalizedUserName));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("DisplayName must not be empty.", nameof(displayName));
        }

        return new ApplicationUser(
            id, userStatusId, userName, normalizedUserName, displayName, isServiceAccount, email,
            normalizedEmail, passwordHash, givenName, familyName, createdAtUtc);
    }

    public void Lock(DateTime lockoutEndUtc) => LockoutEndUtc = lockoutEndUtc;

    public void Unlock()
    {
        LockoutEndUtc = null;
        AccessFailedCount = 0;
    }
}
