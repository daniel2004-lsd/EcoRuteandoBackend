using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler
    : IRequestHandler<GetPermissionsQuery, IReadOnlyList<GetPermissionsResponse>>
{
    private readonly IPermissionRepository _repository;

    public GetPermissionsQueryHandler(
        IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<GetPermissionsResponse>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _repository.GetAllAsync(
            cancellationToken);

        return permissions
            .Select(permission => new GetPermissionsResponse(
                permission.Id,
                permission.Name,
                permission.Description))
            .ToList();
    }
}