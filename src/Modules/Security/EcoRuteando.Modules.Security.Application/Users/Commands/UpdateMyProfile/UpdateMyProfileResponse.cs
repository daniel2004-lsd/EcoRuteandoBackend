namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyProfile;

public sealed record UpdateMyProfileResponse(
    Guid Id,
    string FirstName,
    string? LastName,
    string Email,
    string? PhoneNumber,
    DateTime UpdatedAt);
