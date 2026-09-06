using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeatherCurrent;

public sealed record GetWeatherCurrentQuery(
    double Lat,
    double Lng)
    : IRequest<GetWeatherCurrentResponse>;