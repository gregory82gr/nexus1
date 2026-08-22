using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Organisational unit: operations, engineering, safety, maintenance, security, compliance (atlas C.3.4.4).</summary>
public sealed class Department : Entity<DepartmentId>, IAggregateRoot
{
    private Department(
        DepartmentId id, LegalEntityId legalEntityId, DepartmentId? parentDepartmentId, DepartmentTypeId departmentTypeId,
        string code, string name, string? costCentreCode, DateTime createdAtUtc)
        : base(id)
    {
        LegalEntityId = legalEntityId;
        ParentDepartmentId = parentDepartmentId;
        DepartmentTypeId = departmentTypeId;
        Code = code;
        Name = name;
        CostCentreCode = costCentreCode;
        CreatedAtUtc = createdAtUtc;
    }

    public LegalEntityId LegalEntityId { get; }

    public DepartmentId? ParentDepartmentId { get; }

    public DepartmentTypeId DepartmentTypeId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? CostCentreCode { get; }

    public DateTime CreatedAtUtc { get; }

    public static Department Create(
        DepartmentId id, LegalEntityId legalEntityId, DepartmentTypeId departmentTypeId, string code, string name,
        DateTime createdAtUtc, DepartmentId? parentDepartmentId = null, string? costCentreCode = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Department code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name must not be empty.", nameof(name));
        }

        return new Department(id, legalEntityId, parentDepartmentId, departmentTypeId, code, name, costCentreCode, createdAtUtc);
    }
}
