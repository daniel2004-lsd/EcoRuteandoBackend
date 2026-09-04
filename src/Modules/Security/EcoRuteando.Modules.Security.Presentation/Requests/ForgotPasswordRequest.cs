using System.ComponentModel.DataAnnotations;

namespace EcoRuteando.Modules.Security.Presentation.Requests;

public sealed record ForgotPasswordRequest(
    [EmailAddress] string Email
);
