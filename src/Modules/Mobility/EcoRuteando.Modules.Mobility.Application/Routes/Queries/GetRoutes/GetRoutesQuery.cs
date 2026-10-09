using EcoRuteando.Modules.Mobility.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRoutes;

public sealed record GetRoutesQuery(
    TransportType? TransportType = null,
    bool IncludeInactive = false)
    : IRequest<IReadOnlyList<GetRoutesResponse>>;
