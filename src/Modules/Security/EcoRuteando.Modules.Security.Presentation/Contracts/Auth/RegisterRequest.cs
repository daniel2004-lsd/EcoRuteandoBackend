using System.ComponentModel.DataAnnotations;

namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth
{
    public sealed record RegisterRequest(
    [MaxLength(50)] string FirstName,
    [MaxLength(50)] string? LastName,
    [EmailAddress] string Email,
    [MinLength(8)] [MaxLength(128)] string Password);
}
