using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Queries.GetWeather;

public class GetWeatherQueryHandler : IRequestHandler<GetWeatherQuery, WeatherDto?>
{
    private readonly IWeatherDataRepository _repository;

    public GetWeatherQueryHandler(IWeatherDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeatherDto?> Handle(GetWeatherQuery request, CancellationToken cancellationToken)
    {
        var data = await _repository.GetCurrentByCoordinatesAsync(request.Latitude, request.Longitude, request.MaxDistanceMeters, cancellationToken);
        if (data == null)
            return null;

        return new WeatherDto(
            data.Id,
            data.Location.Y,
            data.Location.X,
            data.City,
            data.TemperatureC,
            data.HumidityPct,
            data.WindSpeedKmh,
            data.Condition,
            data.RainProbability,
            data.Source,
            data.ValidFrom,
            data.ValidUntil);
    }
}
