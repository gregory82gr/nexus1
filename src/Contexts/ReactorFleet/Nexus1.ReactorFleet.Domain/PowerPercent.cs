namespace Nexus1.ReactorFleet.Domain;

/// <summary>
/// Mirrors the Schema Atlas's UnitPowerSnapshot.PowerPercent
/// CHECK(BETWEEN 0 AND 200) exactly — the 200 ceiling (not 100) is the
/// atlas's own stated allowance for demonstrator overload scenarios,
/// kept as-is per ADR-003 rather than "corrected" to 0-100.
/// </summary>
public readonly record struct PowerPercent
{
    public PowerPercent(decimal value)
    {
        if (value < 0m || value > 200m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value), value, "PowerPercent must be between 0 and 200.");
        }

        Value = value;
    }

    public decimal Value { get; }
}
