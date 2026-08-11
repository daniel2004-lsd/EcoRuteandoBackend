namespace EcoRuteando.Modules.Security.Application.Abstractions.Email;

public interface IEmailTemplateService
{
    string LoadTemplate(string templateName);
}