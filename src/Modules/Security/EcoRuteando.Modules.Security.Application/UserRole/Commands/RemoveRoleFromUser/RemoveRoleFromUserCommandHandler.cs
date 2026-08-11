using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.UserRoles.Commands.RemoveRoleFromUser;

public sealed class RemoveRoleFromUserCommandHandler
    : IRequestHandler<RemoveRoleFromUserCommand>
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleFromUserCommandHandler(
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveRoleFromUserCommand request,
        CancellationToken cancellationToken)
    {
        var userRole = await _userRoleRepository.GetAsync(
            request.UserId,
            request.RoleId,
            cancellationToken);

        if (userRole is null)
        {
            throw new NotFoundException(
                "El usuario no tiene asignado ese rol.");
        }

        await _userRoleRepository.DeleteAsync(
            userRole,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}