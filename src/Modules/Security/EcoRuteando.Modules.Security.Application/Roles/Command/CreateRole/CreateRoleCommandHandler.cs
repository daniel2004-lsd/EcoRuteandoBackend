using EcoRuteando.Modules.Security.Application.Roles.Command.CreateRole;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Roles.Commands.CreateRole;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, int>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var existingRole = await _roleRepository.GetByNameAsync(
            request.Name,
            cancellationToken);

        if (existingRole is not null)
        {
            throw new DomainException("Ya existe un rol con ese nombre.");
        }

        var role = new Role(
            request.Name,
            request.Description);

        await _roleRepository.AddAsync(
            role,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return role.Id;
    }
}