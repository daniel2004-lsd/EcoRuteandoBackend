using System.Security.Cryptography;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.SendVerificationEmail;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler
    : IRequestHandler<UpdateMyProfileCommand, UpdateMyProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public UpdateMyProfileCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecurityUnitOfWork unitOfWork,
        ISender sender)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<UpdateMyProfileResponse> Handle(
        UpdateMyProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        // CU12 / RF12.2: Confirmación de identidad obligatoria para usuarios con contraseña
        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword)
                || !_passwordHasher.Verify(
                    request.CurrentPassword,
                    user.PasswordHash))
            {
                throw new DomainException(
                    "La contraseña actual es incorrecta.");
            }
        }

        var newEmail = request.Email.Trim().ToLowerInvariant();
        var emailChanged = newEmail != user.Email;

        if (emailChanged)
        {
            var emailOwner = await _userRepository.GetByEmailAsync(
                newEmail,
                cancellationToken);

            if (emailOwner is not null)
            {
                throw new ConflictException("El correo no está disponible.");
            }
        }

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        await _userRepository.UpdateAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (emailChanged)
        {
            var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            await _sender.Send(
                new SendVerificationEmailCommand(user.Id, user.Email, null, verificationCode),
                cancellationToken);
        }

        return new UpdateMyProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.UpdatedAt ?? DateTime.UtcNow);
    }
}
