using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommandHandler
    : IRequestHandler<ChangeMyPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public ChangeMyPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecurityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ChangeMyPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash)
            || !_passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash))
        {
            throw new DomainException(
                "La contraseña actual es incorrecta.");
        }

        user.ChangePassword(
            _passwordHasher.Hash(request.NewPassword));

        await _userRepository.UpdateAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
