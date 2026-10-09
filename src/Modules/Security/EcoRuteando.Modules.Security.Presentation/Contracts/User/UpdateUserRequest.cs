
namespace EcoRuteando.Modules.Security.Presentation.Contracts.Users;

public sealed record UpdateUserRequest(
    string FirstName,
    string? LastName,
    string? PhoneNumber,
    string? PrimaryColor
);

