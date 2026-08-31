using EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Sustainability.Queries.EstimateSustainability;

public sealed class EstimateSustainabilityQueryHandler
    : IRequestHandler<EstimateSustainabilityQuery, EstimateSustainabilityResponse>
{
    private readonly IGoogleMapsService _googleMapsService;
    private readonly ITransportFactorRepository _transportFactorRepository;

    public EstimateSustainabilityQueryHandler(
        IGoogleMapsService googleMapsService,
        ITransportFactorRepository transportFactorRepository)
    {
        _googleMapsService = googleMapsService;
        _transportFactorRepository = transportFactorRepository;
    }

    public async Task<EstimateSustainabilityResponse> Handle(
        EstimateSustainabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (!PgEnumExtensions.TryFromPgName(
                request.TransportMode,
                out TransportType transportType))
        {
            throw new DomainException("El modo de transporte no es válido.");
        }

        var directions = await _googleMapsService.GetDirectionsAsync(
            request.OriginLat,
            request.OriginLng,
            request.DestinationLat,
            request.DestinationLng,
            ToGoogleTravelMode(transportType),
            cancellationToken);

        if (directions is null || directions.Distance is null || directions.Duration is null)
        {
            throw new DomainException(
                "No se pudo obtener la ruta desde Google Maps para estimar la ruta.");
        }

        var distanceKm = decimal.Round(
            directions.Distance.ValueMeters / 1000m,
            2,
            MidpointRounding.AwayFromZero);

        var estimatedTimeMin = (int)Math.Round(
            directions.Duration.ValueSeconds / 60.0);

        var modeFactor = await _transportFactorRepository
            .GetActiveByTransportTypeAsync(transportType, cancellationToken);

        decimal? co2EmissionsKg = null;
        decimal? co2SavedKg = null;
        decimal? estimatedCalories = null;

        if (modeFactor is not null)
        {
            co2EmissionsKg = decimal.Round(
                distanceKm * modeFactor.Co2FactorKgKm,
                4,
                MidpointRounding.AwayFromZero);

            var carFactor = await _transportFactorRepository
                .GetActiveByTransportTypeAsync(TransportType.Car, cancellationToken);

            if (carFactor is not null && carFactor.Co2FactorKgKm > modeFactor.Co2FactorKgKm)
            {
                var savedPerKm = carFactor.Co2FactorKgKm - modeFactor.Co2FactorKgKm;

                co2SavedKg = decimal.Round(
                    distanceKm * savedPerKm,
                    4,
                    MidpointRounding.AwayFromZero);
            }

            if (modeFactor.CalorieFactorKm is not null)
            {
                estimatedCalories = decimal.Round(
                    distanceKm * modeFactor.CalorieFactorKm.Value,
                    2,
                    MidpointRounding.AwayFromZero);
            }
        }

        return new EstimateSustainabilityResponse(
            transportType.ToPgName(),
            distanceKm,
            estimatedTimeMin,
            co2EmissionsKg,
            co2SavedKg,
            estimatedCalories);
    }

    private static string ToGoogleTravelMode(TransportType transportType)
    {
        return transportType switch
        {
            TransportType.Bike => "bicycling",
            TransportType.Walking => "walking",
            _ => "transit"
        };
    }
}
