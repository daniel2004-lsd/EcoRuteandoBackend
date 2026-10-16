using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetEmailVerificationStatus;

public sealed class GetEmailVerificationStatusQueryHandler
    : IRequestHandler<GetEmailVerificationStatusQuery, EmailVerificationStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public GetEmailVerificationStatusQueryHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<EmailVerificationStatusResponse> Handle(
        GetEmailVerificationStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new EmailVerificationStatusResponse(
                AlreadyVerified: false,
                UserExists: false);
        }

        var user = await _userRepository.GetByEmailAsync(
            request.Email.Trim(),
            cancellationToken);

        if (user is null)
        {
            return new EmailVerificationStatusResponse(
                AlreadyVerified: false,
                UserExists: false);
        }

        return new EmailVerificationStatusResponse(
            AlreadyVerified: user.EmailVerified,
            UserExists: true);
    }
}
