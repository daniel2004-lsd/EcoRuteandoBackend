using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using MediatR;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand>
{
    private readonly IPasswordRecoveryRepository _passwordRecoveryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IPasswordRecoveryRepository passwordRecoveryRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _passwordRecoveryRepository = passwordRecoveryRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256(request.Token);
        var recovery = await _passwordRecoveryRepository.GetByTokenHashAsync(tokenHash);
        if (recovery is null)
        {
            throw new Exception("Invalid token.");
        }

        if (recovery.IsUsed)
        {
            throw new Exception("Token has already been used.");
        }

        if (recovery.ExpiresAt < DateTime.UtcNow)
        {
            throw new Exception("Token has expired.");
        }

        var user = recovery.User;

        if (user is null)
            throw new Exception("Usuario no encontrado.");

        IPAddress? usageIp = null;

        if (!string.IsNullOrWhiteSpace(request.UsageIp))
        {
            IPAddress.TryParse(request.UsageIp, out usageIp);
        }

        var passwordHash = _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(passwordHash);

        recovery.MarkAsUsed(usageIp);

        _passwordRecoveryRepository.Update(recovery);

        await _userRepository.UpdateAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }
}