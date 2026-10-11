namespace EcoRuteando.Modules.Admin.Application.System.Dtos;

public record SystemConfigurationDto(
    short Id,
    string AppName,
    string? AdminEmail,
    string? SupportPhone,
    string? TermsVersion,
    string? TermsUrl,
    string? PrivacyVersion,
    string? PrivacyUrl,
    string? EnabledApis,
    int MapCacheTtlSec,
    int WeatherCacheTtlSec,
    string PrimaryColor,
    string SecondaryColor,
    string? LogoUrl,
    DateTime UpdatedAt,
    Guid? UpdatedBy);
