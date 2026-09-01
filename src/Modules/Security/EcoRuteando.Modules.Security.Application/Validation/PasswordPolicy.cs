using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Application.Validation;

public static class PasswordPolicy
{
    public const int DefaultMinLength = 8;

    public static string? Validate(
        SecurityPolicy? policy,
        string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var minLength = policy is not null && policy.MinPasswordLength > 0
            ? policy.MinPasswordLength
            : DefaultMinLength;

        if (password.Length < minLength)
        {
            return $"La contraseña debe tener al menos {minLength} caracteres.";
        }

        var requireUppercase = policy?.RequireUppercase ?? true;

        if (requireUppercase && !password.Any(char.IsUpper))
        {
            return "La contraseña debe contener al menos una mayúscula.";
        }

        var requireNumbers = policy?.RequireNumbers ?? true;

        if (requireNumbers && !password.Any(char.IsDigit))
        {
            return "La contraseña debe contener al menos un número.";
        }

        var requireSpecialChars = policy?.RequireSpecialChars ?? true;

        if (requireSpecialChars
            && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            return "La contraseña debe contener al menos un carácter especial.";
        }

        return null;
    }
}
