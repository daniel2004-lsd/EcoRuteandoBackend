using EcoRuteando.Modules.Security.Application.Roles.Commands.UpdateRole;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Roles.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler
    : IRequestHandler<UpdateRoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(
            request.RoleId,
            cancellationToken);

        if (role is null)
        {
            throw new DomainException("El rol no existe.");
        }

        role.Update(
            request.Name,
            request.Description);

        await _roleRepository.UpdateAsync(
            role,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}