using System.ComponentModel.DataAnnotations;

namespace EcoRuteando.Modules.Security.Presentation.Requests;

public sealed record UpdateMyProfileRequest(
    [Required] string FirstName,
    string? LastName,
    [Required][EmailAddress] string Email,
    string? PhoneNumber,
    string? CurrentPassword
);
