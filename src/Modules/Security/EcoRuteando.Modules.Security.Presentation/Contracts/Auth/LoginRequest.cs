using System.ComponentModel.DataAnnotations;

namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth
{
    public sealed record LoginRequest(
    [EmailAddress] string Email,
    string Password);
}
