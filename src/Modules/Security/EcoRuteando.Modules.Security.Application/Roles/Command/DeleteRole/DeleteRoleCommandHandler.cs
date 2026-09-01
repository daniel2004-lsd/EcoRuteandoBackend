using EcoRuteando.Modules.Security.Application.Roles.Commands.DeleteRole;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Roles.Commands.DeleteRole;

public sealed class DeleteRoleCommandHandler
    : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(
        IRoleRepository roleRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(
            request.RoleId,
            cancellationToken);

        if (role is null)
        {
            throw new DomainException("El rol no existe.");
        }

        await _roleRepository.DeleteAsync(
            role,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}