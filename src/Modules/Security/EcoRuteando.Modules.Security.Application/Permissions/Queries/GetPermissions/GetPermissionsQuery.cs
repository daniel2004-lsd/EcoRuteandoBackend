using MediatR;

namespace EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissions;

public sealed record GetPermissionsQuery
    : IRequest<IReadOnlyList<GetPermissionsResponse>>;
