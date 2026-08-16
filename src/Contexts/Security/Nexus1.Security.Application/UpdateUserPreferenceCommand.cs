using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Security.Application;

/// <summary>UserPreference's defining behavior (atlas C.2.4.8). Upsert: first call for a user creates the row, later calls update it — matching the table's own one-row-per-user shape.</summary>
public sealed record UpdateUserPreferenceCommand(
    int ApplicationUserId, int LanguageId, int TimeZoneId, string Theme = "System",
    bool ReceiveEmailAlerts = true, bool ReceiveInAppAlerts = true, string? DateFormat = null, string? NumberFormat = null)
    : ICommand;
