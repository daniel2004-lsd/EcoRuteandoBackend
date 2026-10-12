using EcoRuteando.Modules.Admin.Application.System.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.System.Commands.UpdateSystemConfiguration;

public class UpdateSystemConfigurationCommandHandler : IRequestHandler<UpdateSystemConfigurationCommand, SystemConfigurationDto>
{
    private readonly ISystemConfigurationRepository _repository;

    public UpdateSystemConfigurationCommandHandler(ISystemConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemConfigurationDto> Handle(UpdateSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _repository.GetConfigurationAsync(cancellationToken);

        config.Update(
            request.AppName,
            request.AdminEmail,
            request.SupportPhone,
            request.TermsVersion,
            request.TermsUrl,
            request.PrivacyVersion,
            request.PrivacyUrl,
            request.EnabledApis,
            request.MapCacheTtlSec,
            request.WeatherCacheTtlSec,
            request.PrimaryColor,
            request.SecondaryColor,
            request.LogoUrl,
            request.UpdatedBy);

        await _repository.UpdateConfigurationAsync(config, cancellationToken);

        return new SystemConfigurationDto(
            config.Id,
            config.AppName,
            config.AdminEmail,
            config.SupportPhone,
            config.TermsVersion,
            config.TermsUrl,
            config.PrivacyVersion,
            config.PrivacyUrl,
            config.EnabledApis,
            config.MapCacheTtlSec,
            config.WeatherCacheTtlSec,
            config.PrimaryColor,
            config.SecondaryColor,
            config.LogoUrl,
            config.UpdatedAt,
            config.UpdatedBy);
    }
}
