using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetEmailVerificationStatus;

public sealed record GetEmailVerificationStatusQuery(
    string? Email)
    : IRequest<EmailVerificationStatusResponse>;
