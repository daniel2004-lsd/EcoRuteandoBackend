using EcoRuteando.Modules.Mobility.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRoutes;

public sealed record GetRoutesQuery(
    TransportType? TransportType = null,
    bool IncludeInactive = false,
    Guid? CreatedBy = null,
    int Page = 1,
    int PageSize = 50)
    : IRequest<IReadOnlyList<GetRoutesResponse>>;
