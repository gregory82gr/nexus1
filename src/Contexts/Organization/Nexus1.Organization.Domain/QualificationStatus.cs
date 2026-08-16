using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Typed status catalogue for qualification/certification rows (atlas C.3.3): valid, expired, pending, revoked.</summary>
public sealed class QualificationStatus : Entity<QualificationStatusId>, IAggregateRoot
{
    private QualificationStatus(QualificationStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public int DisplayOrder { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public static QualificationStatus Create(
        QualificationStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("QualificationStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("QualificationStatus name must not be empty.", nameof(name));
        }

        return new QualificationStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
