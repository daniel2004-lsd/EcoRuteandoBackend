using EcoRuteando.Modules.Admin.Application.System.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.System.Queries.GetSystemConfiguration;

public class GetSystemConfigurationQueryHandler : IRequestHandler<GetSystemConfigurationQuery, SystemConfigurationDto>
{
    private readonly ISystemConfigurationRepository _repository;

    public GetSystemConfigurationQueryHandler(ISystemConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemConfigurationDto> Handle(GetSystemConfigurationQuery request, CancellationToken cancellationToken)
    {
        var config = await _repository.GetConfigurationAsync(cancellationToken);

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
