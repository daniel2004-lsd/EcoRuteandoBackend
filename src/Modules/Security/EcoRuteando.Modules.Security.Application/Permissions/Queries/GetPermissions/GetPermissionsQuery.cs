using EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissions;
using MediatR;

public sealed record GetPermissionsQuery
    : IRequest<IReadOnlyList<GetPermissionsResponse>>;