using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.ReactorFleet.Domain;

/// <summary>
/// Phase 1 slice only — see ADR-003. Matches From_Domain_to_Twin's bare
/// identity example exactly; the Schema Atlas's Reactor/Equipment/etc.
/// tables are deliberately not modeled yet.
/// </summary>
public sealed class Unit : Entity<UnitId>, IAggregateRoot
{
    private Unit(UnitId id, string code, string name)
        : base(id)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; }

    public string Name { get; }

    public static Unit Create(UnitId id, string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Unit code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Unit name must not be empty.", nameof(name));
        }

        return new Unit(id, code, name);
    }
}
