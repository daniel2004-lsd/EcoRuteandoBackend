using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Permissions.Commands.DeletePermission;

public sealed class DeletePermissionCommandHandler
    : IRequestHandler<DeletePermissionCommand>
{
    private readonly IPermissionRepository _repository;

    public DeletePermissionCommandHandler(
        IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeletePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await _repository.GetByIdAsync(
            request.PermissionId,
            cancellationToken);

        if (permission is null)
        {
            throw new NotFoundException("El permiso no existe.");
        }

        await _repository.DeleteAsync(
            permission,
            cancellationToken);
    }
}