using System.ComponentModel.DataAnnotations;

namespace EcoRuteando.Modules.Security.Presentation.Requests;

public sealed record ResetPasswordRequest(
    string Token,
    [MinLength(8)] [MaxLength(128)] string NewPassword,
    string? Email = null
);
