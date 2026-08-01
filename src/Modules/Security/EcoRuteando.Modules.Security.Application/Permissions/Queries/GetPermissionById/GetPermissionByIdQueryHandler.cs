using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissionById;

public sealed class GetPermissionByIdQueryHandler
    : IRequestHandler<GetPermissionByIdQuery, GetPermissionByIdResponse>
{
    private readonly IPermissionRepository _repository;

    public GetPermissionByIdQueryHandler(
        IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetPermissionByIdResponse> Handle(
        GetPermissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var permission = await _repository.GetByIdAsync(
            request.PermissionId,
            cancellationToken);

        if (permission is null)
        {
            throw new NotFoundException("El permiso no existe.");
        }

        return new GetPermissionByIdResponse(
            permission.Id,
            permission.Name,
            permission.Description);
    }
}