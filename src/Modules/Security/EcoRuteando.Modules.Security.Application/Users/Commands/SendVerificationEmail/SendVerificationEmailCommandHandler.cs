using System.Security.Cryptography;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using System.Net;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.SendVerificationEmail;

public sealed class SendVerificationEmailCommandHandler
    : IRequestHandler<SendVerificationEmailCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IUnitOfWork _unitOfWork;

    public SendVerificationEmailCommandHandler(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IEmailService emailService,
        IEmailTemplateService templateService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _emailService = emailService;
        _templateService = templateService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        SendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = request.UserId is not null
            ? await _userRepository.GetByIdAsync(request.UserId.Value, cancellationToken)
            : string.IsNullOrWhiteSpace(request.Email)
                ? null
                : await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || user.EmailVerified)
        {
            return;
        }

        var existingVerification = await _emailVerificationRepository
            .GetActiveByUserIdAsync(user.Id, cancellationToken);

        if (existingVerification is not null)
        {
            return;
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var tokenHash = ComputeSha256(code);

        IPAddress? requestIp = null;
        if (!string.IsNullOrWhiteSpace(request.RequestIp))
        {
            IPAddress.TryParse(request.RequestIp, out requestIp);
        }

        var verification = new EmailVerification(
            user.Id,
            tokenHash,
            DateTime.UtcNow.AddHours(24),
            requestIp);

        await _emailVerificationRepository.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var body = _templateService.LoadTemplate("VerifyEmail.html");
        body = body.Replace("{{UserName}}", UserNameFormatter.Format(user.FirstName));
        body = body.Replace("{{Code}}", code);

        await _emailService.SendAsync(
            user.Email,
            "Verifica tu correo electrónico - EcoRuteando",
            body);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
