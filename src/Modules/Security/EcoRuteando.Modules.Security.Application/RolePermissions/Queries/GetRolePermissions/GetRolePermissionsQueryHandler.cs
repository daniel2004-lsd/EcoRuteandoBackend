using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Queries.GetRolePermissions;

public sealed class GetRolePermissionsQueryHandler
    : IRequestHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionResponse>>
{
    private readonly IRolePermissionRepository _rolePermissionRepository;

    public GetRolePermissionsQueryHandler(
        IRolePermissionRepository rolePermissionRepository)
    {
        _rolePermissionRepository = rolePermissionRepository;
    }

    public async Task<IReadOnlyList<RolePermissionResponse>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _rolePermissionRepository.GetByRoleIdAsync(
            request.RoleId,
            cancellationToken);

        return permissions
            .Select(rp => new RolePermissionResponse(
                rp.PermissionId,
                rp.Permission.Name))
            .ToList();
    }
}