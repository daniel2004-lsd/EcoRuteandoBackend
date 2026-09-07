using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.SharedRoutes.Queries.GetSharedRoutes;

public sealed record GetSharedRoutesQuery(Guid UserId)
    : IRequest<IReadOnlyList<GetSharedRoutesResponse>>;