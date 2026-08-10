using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Permissions.Commands.UpdatePermission;

public sealed class UpdatePermissionCommandHandler
    : IRequestHandler<UpdatePermissionCommand>
{
    private readonly IPermissionRepository _repository;

    public UpdatePermissionCommandHandler(
        IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        UpdatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await _repository.GetByIdAsync(
            request.PermissionId,
            cancellationToken);

        if (permission is null)
        {
            throw new NotFoundException("El permiso no existe.");
        }

        permission.Update(
            request.Name,
            request.Description);

        await _repository.UpdateAsync(
            permission,
            cancellationToken);
    }
}