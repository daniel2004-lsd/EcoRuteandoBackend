using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Queries.GetAirQuality;

public record GetAirQualityQuery(double Latitude, double Longitude, double MaxDistanceMeters = 50000) : IRequest<AirQualityDto?>;
