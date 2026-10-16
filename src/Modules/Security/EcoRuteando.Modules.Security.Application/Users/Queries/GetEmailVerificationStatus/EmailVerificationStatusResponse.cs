namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetEmailVerificationStatus;

public sealed record EmailVerificationStatusResponse(
    bool AlreadyVerified,
    bool UserExists);
