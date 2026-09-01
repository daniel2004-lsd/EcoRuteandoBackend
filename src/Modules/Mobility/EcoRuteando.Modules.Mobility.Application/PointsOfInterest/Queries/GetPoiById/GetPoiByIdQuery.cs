using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPoiById;

public sealed record GetPoiByIdQuery(
    Guid PoiId)
    : IRequest<GetPoiByIdResponse>;