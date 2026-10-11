using EcoRuteando.Modules.Admin.Application.System.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.System.Commands.UpdateSystemConfiguration;

public record UpdateSystemConfigurationCommand(
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
    Guid? UpdatedBy) : IRequest<SystemConfigurationDto>;
