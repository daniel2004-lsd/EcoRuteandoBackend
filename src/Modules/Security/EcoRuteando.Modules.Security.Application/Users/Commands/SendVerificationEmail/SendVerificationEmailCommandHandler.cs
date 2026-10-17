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
    : IRequestHandler<SendVerificationEmailCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public SendVerificationEmailCommandHandler(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IEmailService emailService,
        IEmailTemplateService templateService,
        ISecurityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _emailService = emailService;
        _templateService = templateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        SendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = request.UserId is not null
            ? await _userRepository.GetByIdAsync(request.UserId.Value, cancellationToken)
            : string.IsNullOrWhiteSpace(request.Email)
                ? null
                : await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return false;
        }

        if (user.EmailVerified)
        {
            return true;
        }

        var existingVerification = await _emailVerificationRepository
            .GetActiveByUserIdAsync(user.Id, cancellationToken);

        var code = request.Code ?? RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var tokenHash = ComputeSha256(code);

        IPAddress? requestIp = null;
        if (!string.IsNullOrWhiteSpace(request.RequestIp))
        {
            IPAddress.TryParse(request.RequestIp, out requestIp);
        }

        if (existingVerification is not null)
        {
            existingVerification.UpdateToken(tokenHash, DateTime.UtcNow.AddHours(24), requestIp);
            _emailVerificationRepository.Update(existingVerification);
        }
        else
        {
            var verification = new EmailVerification(
                user.Id,
                tokenHash,
                DateTime.UtcNow.AddHours(24),
                requestIp);

            await _emailVerificationRepository.AddAsync(verification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var body = _templateService.LoadTemplate("VerifyEmail.html");
        body = body.Replace("{{UserName}}", UserNameFormatter.Format(user.FirstName));
        body = body.Replace("{{Code}}", code);

        await _emailService.SendAsync(
            user.Email,
            "Verifica tu correo electrónico - EcoRuteando",
            body);

        return false;
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
