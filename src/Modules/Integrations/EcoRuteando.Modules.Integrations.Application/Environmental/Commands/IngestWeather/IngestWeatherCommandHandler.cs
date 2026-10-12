using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using EcoRuteando.Modules.Integrations.Domain.Entities;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestWeather;

public class IngestWeatherCommandHandler : IRequestHandler<IngestWeatherCommand, WeatherDto>
{
    private readonly IWeatherDataRepository _repository;

    public IngestWeatherCommandHandler(IWeatherDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeatherDto> Handle(IngestWeatherCommand request, CancellationToken cancellationToken)
    {
        var entity = WeatherData.Create(
            request.Latitude,
            request.Longitude,
            request.City,
            request.TemperatureC,
            request.HumidityPct,
            request.WindSpeedKmh,
            request.Condition,
            request.RainProbability,
            request.RawData,
            request.Source,
            request.ValidFrom,
            request.ValidUntil);

        await _repository.AddAsync(entity, cancellationToken);

        return new WeatherDto(
            entity.Id,
            entity.Location.Y,
            entity.Location.X,
            entity.City,
            entity.TemperatureC,
            entity.HumidityPct,
            entity.WindSpeedKmh,
            entity.Condition,
            entity.RainProbability,
            entity.Source,
            entity.ValidFrom,
            entity.ValidUntil);
    }
}
