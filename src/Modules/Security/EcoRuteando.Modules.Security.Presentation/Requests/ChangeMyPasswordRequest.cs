using System.ComponentModel.DataAnnotations;

namespace EcoRuteando.Modules.Security.Presentation.Requests;

public sealed record ChangeMyPasswordRequest(
    [Required] string CurrentPassword,
    [Required][MinLength(8)][MaxLength(128)] string NewPassword
);
