namespace EcoRuteando.Modules.Security.Application.Abstractions.Email;

public static class UserNameFormatter
{
    public static string Format(string? firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return string.Empty;
        }

        var firstWord = firstName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

        return char.ToUpperInvariant(firstWord[0]) + firstWord[1..].ToLowerInvariant();
    }
}
