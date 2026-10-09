using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripById;

public sealed record GetTripByIdQuery(
    Guid UsageId,
    Guid UserId)
    : IRequest<GetTripByIdResponse>;
