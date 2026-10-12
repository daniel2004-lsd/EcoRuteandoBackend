using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Queries.GetAirQuality;

public class GetAirQualityQueryHandler : IRequestHandler<GetAirQualityQuery, AirQualityDto?>
{
    private readonly IAirQualityDataRepository _repository;

    public GetAirQualityQueryHandler(IAirQualityDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<AirQualityDto?> Handle(GetAirQualityQuery request, CancellationToken cancellationToken)
    {
        var data = await _repository.GetLatestByCoordinatesAsync(request.Latitude, request.Longitude, request.MaxDistanceMeters, cancellationToken);
        if (data == null)
            return null;

        var category = data.Aqi switch
        {
            <= 50 => "Good",
            <= 100 => "Moderate",
            <= 150 => "Unhealthy for Sensitive Groups",
            <= 200 => "Unhealthy",
            <= 300 => "Very Unhealthy",
            > 300 => "Hazardous",
            _ => "Unknown"
        };

        return new AirQualityDto(
            data.Id,
            data.Location.Y,
            data.Location.X,
            data.City,
            data.Aqi,
            category,
            data.Pm25,
            data.Pm10,
            data.Co2Ppm,
            data.Source,
            data.MeasuredAt);
    }
}
