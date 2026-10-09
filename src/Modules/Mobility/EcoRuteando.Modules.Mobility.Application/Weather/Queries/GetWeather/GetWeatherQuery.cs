using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeather;

public sealed record GetWeatherQuery(
    double OriginLat,
    double OriginLng,
    double DestinationLat,
    double DestinationLng)
    : IRequest<GetWeatherResponse>;