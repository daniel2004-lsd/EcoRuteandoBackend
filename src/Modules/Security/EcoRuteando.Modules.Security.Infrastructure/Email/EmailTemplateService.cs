using EcoRuteando.Modules.Security.Application.Abstractions.Email;

public sealed class EmailTemplateService : IEmailTemplateService
{
    public string LoadTemplate(string templateName)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Email",
            "Templates",
            templateName);

        return File.ReadAllText(path);
    }
}