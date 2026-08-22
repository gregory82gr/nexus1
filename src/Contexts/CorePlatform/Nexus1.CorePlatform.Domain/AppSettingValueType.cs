namespace Nexus1.CorePlatform.Domain;

/// <summary>Matches the atlas's CK_CorePlatform_AppSetting_ValueType check constraint exactly (C.1.4.2).</summary>
public enum AppSettingValueType
{
    String,
    Int,
    Bool,
    Json,
}
