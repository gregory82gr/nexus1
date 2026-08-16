using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Security.Domain;

/// <summary>
/// One-to-one user preference row (atlas C.2.4.8) consuming CorePlatform
/// language and time-zone passports by id only, not a real FK — Security
/// lives in its own physical database (SecurityDb, ADR-016), so ADR-015's
/// real-cross-schema-FK exception for reference data cannot apply across a
/// database boundary; a plain passport id is the correct shape here, same
/// as any genuine cross-database reference elsewhere in this project. The
/// primary key IS the owning user's id (one row per user), so this entity
/// uses Entity&lt;ApplicationUserId&gt; directly rather than a separate
/// surrogate id.
/// </summary>
public sealed class UserPreference : Entity<ApplicationUserId>, IAggregateRoot
{
    private UserPreference(
        ApplicationUserId id, int languageId, int timeZoneId, string theme, bool receiveEmailAlerts,
        bool receiveInAppAlerts, string? dateFormat, string? numberFormat, DateTime createdAtUtc)
        : base(id)
    {
        LanguageId = languageId;
        TimeZoneId = timeZoneId;
        Theme = theme;
        ReceiveEmailAlerts = receiveEmailAlerts;
        ReceiveInAppAlerts = receiveInAppAlerts;
        DateFormat = dateFormat;
        NumberFormat = numberFormat;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>CorePlatform.Language passport id — no enforced FK; CorePlatform lives in a different physical database (ADR-016).</summary>
    public int LanguageId { get; private set; }

    /// <summary>CorePlatform.TimeZone passport id — see LanguageId's own note.</summary>
    public int TimeZoneId { get; private set; }

    public string Theme { get; private set; }

    public string? DateFormat { get; private set; }

    public string? NumberFormat { get; private set; }

    public bool ReceiveEmailAlerts { get; private set; }

    public bool ReceiveInAppAlerts { get; private set; }

    public DateTime CreatedAtUtc { get; }

    private static readonly string[] ValidThemes = ["Light", "Dark", "System"];

    public static UserPreference Create(
        ApplicationUserId id, int languageId, int timeZoneId, DateTime createdAtUtc, string theme = "System",
        bool receiveEmailAlerts = true, bool receiveInAppAlerts = true, string? dateFormat = null, string? numberFormat = null)
    {
        if (!ValidThemes.Contains(theme))
        {
            throw new ArgumentException("Theme must be one of Light, Dark, System.", nameof(theme));
        }

        return new UserPreference(id, languageId, timeZoneId, theme, receiveEmailAlerts, receiveInAppAlerts, dateFormat, numberFormat, createdAtUtc);
    }

    public void Update(int languageId, int timeZoneId, string theme, bool receiveEmailAlerts, bool receiveInAppAlerts, string? dateFormat, string? numberFormat)
    {
        if (!ValidThemes.Contains(theme))
        {
            throw new ArgumentException("Theme must be one of Light, Dark, System.", nameof(theme));
        }

        LanguageId = languageId;
        TimeZoneId = timeZoneId;
        Theme = theme;
        ReceiveEmailAlerts = receiveEmailAlerts;
        ReceiveInAppAlerts = receiveInAppAlerts;
        DateFormat = dateFormat;
        NumberFormat = numberFormat;
    }
}
