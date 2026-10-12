namespace EcoRuteando.Modules.Admin.Domain.Entities;

public class SystemConfiguration
{
    public short Id { get; private set; } = 1;
    public string AppName { get; private set; } = "EcoRuteando";
    public string? AdminEmail { get; private set; }
    public string? SupportPhone { get; private set; }
    public string? TermsVersion { get; private set; }
    public string? TermsUrl { get; private set; }
    public string? PrivacyVersion { get; private set; }
    public string? PrivacyUrl { get; private set; }
    public string? EnabledApis { get; private set; }
    public int MapCacheTtlSec { get; private set; } = 3600;
    public int WeatherCacheTtlSec { get; private set; } = 1800;
    public string PrimaryColor { get; private set; } = "#2E7D32";
    public string SecondaryColor { get; private set; } = "#66BB6A";
    public string? LogoUrl { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; private set; }

    private SystemConfiguration() { }

    public static SystemConfiguration CreateDefault()
    {
        return new SystemConfiguration
        {
            Id = 1,
            AppName = "EcoRuteando",
            MapCacheTtlSec = 3600,
            WeatherCacheTtlSec = 1800,
            PrimaryColor = "#2E7D32",
            SecondaryColor = "#66BB6A",
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string appName,
        string? adminEmail,
        string? supportPhone,
        string? termsVersion,
        string? termsUrl,
        string? privacyVersion,
        string? privacyUrl,
        string? enabledApis,
        int mapCacheTtlSec,
        int weatherCacheTtlSec,
        string primaryColor,
        string secondaryColor,
        string? logoUrl,
        Guid? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(appName))
            throw new ArgumentException("AppName cannot be empty.", nameof(appName));

        AppName = appName.Trim();
        AdminEmail = string.IsNullOrWhiteSpace(adminEmail) ? null : adminEmail.Trim();
        SupportPhone = string.IsNullOrWhiteSpace(supportPhone) ? null : supportPhone.Trim();
        TermsVersion = string.IsNullOrWhiteSpace(termsVersion) ? null : termsVersion.Trim();
        TermsUrl = string.IsNullOrWhiteSpace(termsUrl) ? null : termsUrl.Trim();
        PrivacyVersion = string.IsNullOrWhiteSpace(privacyVersion) ? null : privacyVersion.Trim();
        PrivacyUrl = string.IsNullOrWhiteSpace(privacyUrl) ? null : privacyUrl.Trim();
        EnabledApis = enabledApis;
        MapCacheTtlSec = mapCacheTtlSec > 0 ? mapCacheTtlSec : 3600;
        WeatherCacheTtlSec = weatherCacheTtlSec > 0 ? weatherCacheTtlSec : 1800;
        PrimaryColor = !string.IsNullOrWhiteSpace(primaryColor) ? primaryColor.Trim() : "#2E7D32";
        SecondaryColor = !string.IsNullOrWhiteSpace(secondaryColor) ? secondaryColor.Trim() : "#66BB6A";
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
