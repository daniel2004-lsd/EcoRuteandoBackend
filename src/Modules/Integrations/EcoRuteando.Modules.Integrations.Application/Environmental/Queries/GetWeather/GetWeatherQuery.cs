using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Queries.GetWeather;

public record GetWeatherQuery(double Latitude, double Longitude, double MaxDistanceMeters = 50000) : IRequest<WeatherDto?>;
