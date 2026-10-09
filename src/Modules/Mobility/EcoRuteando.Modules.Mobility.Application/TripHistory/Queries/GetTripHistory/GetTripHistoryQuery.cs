using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripHistory;

public sealed record GetTripHistoryQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<GetTripHistoryResponse>>;
