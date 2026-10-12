using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using EcoRuteando.Modules.Integrations.Domain.Entities;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestAirQuality;

public class IngestAirQualityCommandHandler : IRequestHandler<IngestAirQualityCommand, AirQualityDto>
{
    private readonly IAirQualityDataRepository _repository;

    public IngestAirQualityCommandHandler(IAirQualityDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<AirQualityDto> Handle(IngestAirQualityCommand request, CancellationToken cancellationToken)
    {
        var entity = AirQualityData.Create(
            request.Latitude,
            request.Longitude,
            request.City,
            request.Aqi,
            request.Pm25,
            request.Pm10,
            request.Co2Ppm,
            request.Source,
            request.RawData,
            request.MeasuredAt);

        await _repository.AddAsync(entity, cancellationToken);

        var category = entity.Aqi switch
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
            entity.Id,
            entity.Location.Y,
            entity.Location.X,
            entity.City,
            entity.Aqi,
            category,
            entity.Pm25,
            entity.Pm10,
            entity.Co2Ppm,
            entity.Source,
            entity.MeasuredAt);
    }
}
