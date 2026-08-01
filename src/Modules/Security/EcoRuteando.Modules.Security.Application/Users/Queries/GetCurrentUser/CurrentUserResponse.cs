namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;

public sealed record CurrentUserResponse(
    Guid Id,
    string FirstName,
    string? LastName,
    string Email,
    string Role);