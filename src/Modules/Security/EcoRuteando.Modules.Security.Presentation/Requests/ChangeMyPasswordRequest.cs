using System.ComponentModel.DataAnnotations;

public sealed record ChangeMyPasswordRequest(
    [Required] string CurrentPassword,
    [Required] [MinLength(8)] [MaxLength(128)] string NewPassword
);
