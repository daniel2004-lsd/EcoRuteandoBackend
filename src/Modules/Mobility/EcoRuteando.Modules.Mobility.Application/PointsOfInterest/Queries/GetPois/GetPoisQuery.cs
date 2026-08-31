using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPois;

public sealed record GetPoisQuery(
    string? PoiType = null)
    : IRequest<IReadOnlyList<GetPoisResponse>>;