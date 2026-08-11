using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Permissions.Commands.CreatePermission;

public sealed class CreatePermissionCommandHandler
    : IRequestHandler<CreatePermissionCommand, int>
{
    private readonly IPermissionRepository _repository;

    public CreatePermissionCommandHandler(
        IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(
        CreatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var existingPermission = await _repository.GetByNameAsync(
            request.Name,
            cancellationToken);

        if (existingPermission is not null)
        {
            throw new ConflictException(
                "Ya existe un permiso con ese nombre.");
        }

        var permission = new Permission(
            request.Name,
            request.Description);

        await _repository.AddAsync(
            permission,
            cancellationToken);

        return permission.Id;
    }
}