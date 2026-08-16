namespace Nexus1.DigitalTwin.Application;

/// <summary>One variable value inside a CaptureTwinSnapshotCommand. Exactly one of NumericValue/TextValue/JsonValue must be supplied (CK_DigitalTwin_TwinSnapshotValue_OneValue).</summary>
public sealed record TwinSnapshotValueRequest(
    int TwinVariableId, double? NumericValue = null, string? TextValue = null, string? JsonValue = null);
