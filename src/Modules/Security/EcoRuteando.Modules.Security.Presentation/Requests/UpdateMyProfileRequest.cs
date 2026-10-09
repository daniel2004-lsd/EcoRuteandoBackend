using System.ComponentModel.DataAnnotations;

public sealed record UpdateMyProfileRequest(
    [Required] string FirstName,
    string? LastName,
    [Required] [EmailAddress] string Email,
    string? PhoneNumber,
    [Required] string CurrentPassword
);
