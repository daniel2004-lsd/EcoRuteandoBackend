namespace EcoRuteando.Modules.Security.Infrastructure.Bootstrap;

public sealed class AdminBootstrapOptions
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = "Administrador";

    public string? LastName { get; set; }
}