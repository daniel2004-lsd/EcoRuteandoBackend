
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;


namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
     IUserRepository userRepository,
     IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        user.Update(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.PrimaryColor);

        await _userRepository.UpdateAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

