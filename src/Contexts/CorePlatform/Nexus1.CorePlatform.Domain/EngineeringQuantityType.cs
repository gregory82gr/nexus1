namespace Nexus1.CorePlatform.Domain;

/// <summary>
/// Matches the atlas's CK_CorePlatform_EngineeringUnit_QuantityType check
/// constraint exactly (C.1.4.8). Named EngineeringQuantityType, not
/// QuantityType — generic enough a name to risk colliding with a future
/// sector's own "QuantityType" concept once cross-schema references exist.
/// </summary>
public enum EngineeringQuantityType
{
    Power,
    ThermalPower,
    Temperature,
    Pressure,
    Reactivity,
    Flow,
    Frequency,
    Percentage,
    RadiationDoseRate,
    RadiationDose,
    CountRate,
    Mass,
    Time,
    Other,
}
