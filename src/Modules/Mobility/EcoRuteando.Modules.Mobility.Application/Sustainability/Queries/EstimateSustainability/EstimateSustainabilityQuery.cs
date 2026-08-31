using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Sustainability.Queries.EstimateSustainability;

public sealed record EstimateSustainabilityQuery(
    double OriginLat,
    double OriginLng,
    double DestinationLat,
    double DestinationLng,
    string TransportMode)
    : IRequest<EstimateSustainabilityResponse>;
