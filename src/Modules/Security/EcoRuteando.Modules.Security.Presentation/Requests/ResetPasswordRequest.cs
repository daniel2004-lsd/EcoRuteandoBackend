using System.ComponentModel.DataAnnotations;

public sealed record ResetPasswordRequest(
    string Token,
    [MinLength(8)] [MaxLength(128)] string NewPassword
);
