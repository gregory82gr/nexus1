namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// Passport reference to Security.ApplicationUser — Security is out of Phase-1
/// scope, so no assumption is made about its actual primary-key shape (ADR-004).
/// </summary>
public readonly record struct UserId(Guid Value);
