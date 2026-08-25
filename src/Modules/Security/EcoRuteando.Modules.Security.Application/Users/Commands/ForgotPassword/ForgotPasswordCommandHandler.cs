using System.Security.Cryptography;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using System.Net;

using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordRecoveryRepository _passwordRecoveryRepository;
    private readonly IOtpProvider _otpProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IAuditLogService _auditLogService;

    public ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordRecoveryRepository passwordRecoveryRepository,
    IUnitOfWork unitOfWork,
    IOtpProvider otpProvider,
    IEmailService emailService,
    IEmailTemplateService templateService,
    IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _passwordRecoveryRepository = passwordRecoveryRepository;
        _otpProvider = otpProvider;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _templateService = templateService;
        _auditLogService = auditLogService;
    }

    public async Task Handle(
    ForgotPasswordCommand request,
    CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            return;

        var code = _otpProvider.GenerateCode();

        var tokenHash = ComputeSha256(code);

        // Convertir string -> IPAddress
        IPAddress? requestIp = null;

        if (!string.IsNullOrWhiteSpace(request.RequestIp))
        {
            IPAddress.TryParse(request.RequestIp, out requestIp);
        }

        var recovery = new PasswordRecovery(
            user.Id,
            tokenHash,
            DateTime.UtcNow.AddHours(1),
            requestIp);

        await _passwordRecoveryRepository.AddAsync(recovery);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var body = _templateService.LoadTemplate("ForgotPassword.html");

        body = body.Replace("{{UserName}}", UserNameFormatter.Format(user.FirstName));
        body = body.Replace("{{Code}}", code);

        await _emailService.SendAsync(
            user.Email,
            "Recuperación de contraseña - EcoRuteando",
            body);

        await _auditLogService.LogAsync(
            user.Id,
            "user.password_recovery_requested",
            entityName: "users",
            entityId: user.Id.ToString(),
            sourceIp: request.RequestIp,
            cancellationToken: cancellationToken);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }
}