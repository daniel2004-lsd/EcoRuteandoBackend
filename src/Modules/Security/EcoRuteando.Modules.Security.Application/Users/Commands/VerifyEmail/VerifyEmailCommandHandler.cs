using System.Security.Cryptography;
using System.Text;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler
    : IRequestHandler<VerifyEmailCommand>
{
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IEmailVerificationRepository emailVerificationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _emailVerificationRepository = emailVerificationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256(request.Code);

        var verification = await _emailVerificationRepository
            .GetByTokenHashAsync(tokenHash, cancellationToken);

        if (verification is null)
        {
            throw new UnauthorizedException("Código de verificación inválido.");
        }

        if (verification.IsVerified)
        {
            throw new ConflictException("Este código ya fue utilizado.");
        }

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("El código de verificación ha expirado.");
        }

        verification.MarkAsVerified();
        _emailVerificationRepository.Update(verification);

        var user = verification.User;
        if (user is not null)
        {
            user.VerifyEmail();
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
